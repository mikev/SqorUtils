using System;
using System.Security.Cryptography;
using System.IO;
using System.Text;

namespace Sqor.Utils.Encryption
{
    public class Crypto
    {
        /// <summary>
        /// The encoding of the final string produced after the encryption
        /// </summary>
        public enum Encoding
        {
            // String formatted with bytes as pairs of hex characters
            Encoded_As_Hex,

            // Normal UTF8 string
            Encoded_As_UTF8,

            // Base 64 encoding
            Encoded_As_Base64
        }

        private byte[] key;
        private byte[] iv;
        private EncryptionAlgorithm algorithm;

        /// <summary>
        /// The encoding of the encrypted string
        /// </summary>
        public Encoding StringEncoding { get; set; }

        public Crypto(string key)
        {
            Rfc2898DeriveBytes rfc = new Rfc2898DeriveBytes(key, System.Text.Encoding.UTF8.GetBytes(key));
            this.key = rfc.GetBytes(8);
            this.iv = this.key;
            this.algorithm = EncryptionAlgorithm.Des;

            StringEncoding = Encoding.Encoded_As_UTF8;
        }

        public Crypto(byte[] key, byte[] iv, EncryptionAlgorithm algorithm = EncryptionAlgorithm.Des)
        {
            StringEncoding = Encoding.Encoded_As_UTF8;

            this.key = key;
            this.iv = iv;
            this.algorithm = algorithm;
        }

        public string Encrypt(string s)
        {
            Encryptor encryptor = new Encryptor(this, algorithm);
            return encryptor.Encrypt(s, key, iv);
        }

        public string Decrypt(string s)
        {
            Decryptor decryptor = new Decryptor(this, algorithm);
            return decryptor.Decrypt(s, key, iv);
        }
    }

    /// <summary>
    /// The Encryptor class handles encryption of a string using one of the 4 
    /// symmetric algorithms - 3DES, DES, RC2 or Rijndael. 
    /// </summary>
    /// <remarks>
    /// When creating an
    /// instance of this class, you must pass the algorithm to use in the 
    /// constructor: see <see cref="EncryptionAlgorithm"/>. Optionally, you can set the output to be formatted as
    /// hex through the FormatAsHex property.
    /// 
    /// This class is based on principles from the Patterns and Practices guide: 
    /// How To: Create an Encryption Library 
    /// by J.D. Meier, Alex Mackman, Michael Dunner, and Srinath Vasireddy 
    /// Microsoft Corporation</remarks>
    public class Encryptor
    {
        private byte[] encKey;
        private byte[] initVec;
        private EncryptTransformer transformer;
        private Crypto crypto;

        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="algId">The encryption algorithm to use</param>
        public Encryptor(Crypto crypto, EncryptionAlgorithm algId)
        {
            this.crypto = crypto;
            transformer = new EncryptTransformer(algId);
        }

        /// <summary>
        /// The current Initial Vector used in Encryption
        /// </summary>
        public string IV
        {
            get { return System.Text.Encoding.UTF8.GetString(initVec); }
        }

        /// <summary>
        /// The current Key being used in Encryption
        /// </summary>
        public string Key
        {
            get { return System.Text.Encoding.UTF8.GetString(encKey); }
        }

        /// <summary>
        /// Encrypts a string using the algorithm supplied in
        /// the class constructor using the provided key and
        /// initial vector.
        /// </summary>
        /// <remarks>Note that the length and composition of the key and IV
        /// are algorithm dependent. For example, 3DES requires a 16 or 24 byte key,
        /// whereas DES requires an 8 byte key. See <see cref="CreateKey"/>
        /// and <see cref="CreateIV"/></remarks>
        /// <param name="StringToEncrypt">The string to be encrypted</param>
        /// <param name="key">The key to use for encryption. The same key will
        /// be used for decryption.</param>
        /// <param name="IV">THe Initial Vector to use. The same IV will be
        /// used for decryption</param>
        /// <returns>An encrypted string. If FormatAsHex has been set,
        /// the string will be hex encoded.</returns>
        public string Encrypt(string StringToEncrypt, byte[] key, byte[] IV)
        {
            string cipherText;
            try
            {
                byte[] abPlainText = System.Text.Encoding.UTF8.GetBytes(StringToEncrypt);
                byte[] bytesKey = key;
                initVec = IV;

                MemoryStream memStreamEncryptedData = new MemoryStream();

                transformer.IV = initVec;
                ICryptoTransform transform =
                    transformer.GetCryptoServiceProvider(bytesKey);

                CryptoStream cs_base64 = new CryptoStream(memStreamEncryptedData, new ToBase64Transform(),
                    CryptoStreamMode.Write);

                CryptoStream encStream = new CryptoStream(cs_base64, transform, CryptoStreamMode.Write);

                //Encrypt the data, write it to the memory stream.

                encStream.Write(abPlainText, 0, abPlainText.Length);

                encKey = transformer.Key;
                initVec = transformer.IV;
                encStream.FlushFinalBlock();
                encStream.Close();

                // Get encrypted string from memory stream
                cipherText = CreateCipherString(memStreamEncryptedData.ToArray());
            }
            catch (Exception ex)
            {
                throw new SecurityException("Error while writing encrypted data to the stream: \n"
                    + ex.Message, ex);
            }
            return cipherText;
        }


        /// <summary>
        /// Creates a string from the encrypted byte array. If
        /// FormatAsHex is true, a hex formattted string will
        /// be returned. Formatting as hex can make storage in a
        /// database simpler as only legal characters will be output.
        /// </summary>
        /// <param name="BytesToConvert">The array of encrypted bytes</param>
        /// <returns>The encrypted string.</returns>
        private string CreateCipherString(byte[] BytesToConvert)
        {
            string cipherText;

            // Get the data back from the byte array, and into a string
            switch (crypto.StringEncoding)
            {
                case Crypto.Encoding.Encoded_As_UTF8:
                    cipherText = System.Text.Encoding.UTF8.GetString(BytesToConvert);
                    break;
                case Crypto.Encoding.Encoded_As_Hex:
                    var ret = new StringBuilder();
                    foreach (byte b in BytesToConvert)
                    {
                        //Format as hex
                        ret.AppendFormat("{0:X2}", b);
                    }
                    cipherText = ret.ToString();
                    break;
                case Crypto.Encoding.Encoded_As_Base64:
                    cipherText = Convert.ToBase64String(BytesToConvert);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return cipherText;
        }

        /// <summary>
        /// A new key which can be used for encryption.
        /// </summary>
        /// <remarks>This method allows the creation of valid keys
        /// for the selcted algorithm. The format and length of a key
        /// is dependent on the algorithm, so use this method to get
        /// a valid key and then use this key in the Encrypt / Decrypt
        /// methods.</remarks>
        /// <returns>A new, valid, algorithm specific key as a string.</returns>
        public string CreateKey()
        {
            transformer.GetCryptoServiceProvider(null);
            byte[] bytesKey = transformer.Key;

            string newKey = System.Text.Encoding.ASCII.GetString(bytesKey);
            return newKey;
        }

        /// <summary>
        /// A new Initial Vector to use for encryption
        /// </summary>
        /// <remarks>This method allows the creation of valid Initial
        /// Vectors for the selcted algorithm. The format and length of an IV
        /// is dependent on the algorithm, so use this method to get
        /// a valid IV and then use this key in the Encrypt / Decrypt
        /// methods.</remarks>
        /// <returns>A new, valid, algorithm dependent IV as a string</returns>
        public string CreateIV()
        {
            transformer.GetCryptoServiceProvider(null);
            byte[] bytesIV = transformer.IV;

            string newIV = System.Text.Encoding.ASCII.GetString(bytesIV);
            return newIV;
        }
    }

    /// <summary>
    /// Inner exception thrown by methods in this class
    /// </summary>
    [Serializable]
    public class SecurityException : ApplicationException
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Message describing the exception</param>
        public SecurityException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Message describing the exception</param>
        /// <param name="innerException">The exception that is the cause of the current exception</param>
        public SecurityException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// The Encryption algorithm to use
    /// </summary>
    public enum EncryptionAlgorithm
    {
        /// <summary>
        /// Use <see cref="DES"/> 
        /// </summary>
        Des = 1,
        /// <summary>
        /// Use <see cref="RC2"/>
        /// </summary>
        Rc2,
        /// <summary>
        /// Use <see cref="Rijndael"/>
        /// </summary>
        Rijndael,
        /// <summary>
        /// Use <see cref="TripleDES"/>
        /// </summary>
        TripleDes
    } ;

    /// <summary>
    /// This class is used to return a Crypto Provider of the type
    /// specified.
    /// 
    /// </summary>
    /// <remarks>
    /// This class is based on the principles from Patterns and Practices guide: 
    /// How To: Create an Encryption Library 
    /// by J.D. Meier, Alex Mackman, Michael Dunner, and Srinath Vasireddy 
    /// Microsoft Corporation
    /// </remarks>
    internal class EncryptTransformer
    {
        /// <summary>
        /// The Algorithn to use
        /// </summary>
        private EncryptionAlgorithm algorithmID;

        /// <summary>
        /// The encryption key to use
        /// </summary>
        private byte[] encKey;

        /// <summary>
        /// The Initial vector to use
        /// </summary>
        private byte[] initVec;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="algId">The Algorithm to use</param>
        internal EncryptTransformer(EncryptionAlgorithm algId)
        {
            //Save the algorithm being used.
            algorithmID = algId;
        }

        /// <summary>
        ///  The IV as an array of bytes
        /// </summary>
        internal byte[] IV
        {
            get { return initVec; }
            set { initVec = value; }
        }

        /// <summary>
        /// THe key as an array of bytes
        /// </summary>
        internal byte[] Key
        {
            get { return encKey; }
        }

        /// <summary>
        /// Returns the proper Service Provider. Sets the key and IV of
        /// the provider. If no key and/or IV were passed, a random key and/or
        /// IV are created. They can then be retrieved from the <see cref="Encryptor.Key"/>
        /// and <see cref="Encryptor.IV"/> properties. (Do not call <see cref="Encryptor.CreateKey()"/> 
        /// or <see cref="Encryptor.CreateIV()"/> as
        /// these create <i>new</i> keys).
        /// </summary>
        /// <param name="bytesKey"></param>
        /// <returns></returns>
        internal ICryptoTransform GetCryptoServiceProvider(byte[] bytesKey)
        {
            // Pick the provider.
            switch (algorithmID)
            {
                case EncryptionAlgorithm.Des:
                    {
                        DES des = new DESCryptoServiceProvider();
                        des.Mode = CipherMode.CBC;

                        // See if a key was provided
                        if (null == bytesKey)
                        {
                            encKey = des.Key;
                        }
                        else
                        {
                            des.Key = bytesKey;
                            encKey = des.Key;
                        }
                        // See if the client provided an initialization vector
                        if (null == initVec)
                        {
                            // Have the algorithm create one
                            initVec = des.IV;
                        }
                        else
                        {
                            //No, give it to the algorithm
                            des.IV = initVec;
                        }
                        return des.CreateEncryptor();
                    }
                case EncryptionAlgorithm.TripleDes:
                    {
                        TripleDES des3 = new TripleDESCryptoServiceProvider();
                        des3.Mode = CipherMode.CBC;
                        // See if a key was provided
                        if (null == bytesKey)
                        {
                            encKey = des3.Key;
                        }
                        else
                        {
                            des3.Key = bytesKey;
                            encKey = des3.Key;
                        }
                        // See if the client provided an IV
                        if (null == initVec)
                        {
                            //Yes, have the alg create one
                            initVec = des3.IV;
                        }
                        else
                        {
                            //No, give it to the alg.
                            des3.IV = initVec;
                        }
                        return des3.CreateEncryptor();
                    }
                case EncryptionAlgorithm.Rc2:
                    {
                        RC2 rc2 = new RC2CryptoServiceProvider();
                        rc2.Mode = CipherMode.CBC;
                        // Test to see if a key was provided
                        if (null == bytesKey)
                        {
                            encKey = rc2.Key;
                        }
                        else
                        {
                            rc2.Key = bytesKey;
                            encKey = rc2.Key;
                        }
                        // See if the client provided an IV
                        if (null == initVec)
                        {
                            //Yes, have the alg create one
                            initVec = rc2.IV;
                        }
                        else
                        {
                            //No, give it to the alg.
                            rc2.IV = initVec;
                        }
                        return rc2.CreateEncryptor();
                    }
                case EncryptionAlgorithm.Rijndael:
                    {
                        Rijndael rijndael = new RijndaelManaged();
                        rijndael.Mode = CipherMode.CBC;
                        // Test to see if a key was provided
                        if (null == bytesKey)
                        {
                            encKey = rijndael.Key;
                        }
                        else
                        {
                            rijndael.Key = bytesKey;
                            encKey = rijndael.Key;
                        }
                        // See if the client provided an IV
                        if (null == initVec)
                        {
                            //Yes, have the alg create one
                            initVec = rijndael.IV;
                        }
                        else
                        {
                            //No, give it to the alg.
                            rijndael.IV = initVec;
                        }
                        return rijndael.CreateEncryptor();
                    }
                default:
                    {
                        throw new CryptographicException("Algorithm ID '" +
                                                         algorithmID +
                                                         "' not supported.");
                    }
            }
        }
    }

    /// <summary>
    /// This class is used to decrypt strings using one of the 4 
    /// symmetric algorithms - 3DES, DES, RC2 or Rijndael.
    /// </summary>
    /// <remarks>
    ///  When creating an
    /// instance of this class, you must pass the algorithm to use in the 
    /// constructor: see <see cref="EncryptionAlgorithm"/>.  Optionally, if the encrypted string ws hex formatted,
    /// you can reformat the input string before decrypting.
    /// 
    /// This class is based on principles from the Patterns and Practices guide: 
    /// How To: Create an Encryption Library 
    /// by J.D. Meier, Alex Mackman, Michael Dunner, and Srinath Vasireddy 
    /// Microsoft Corporation
    ///</remarks>
    public class Decryptor
    {
        private byte[] initVec;
        private DecryptTransformer transformer;
        private Crypto crypto;

        /// <summary>
        /// The constructor
        /// </summary>
        /// <param name="algId">The Algorithm to use</param>
        public Decryptor(Crypto crypto, EncryptionAlgorithm algId)
        {
            //Create a Transformer with the proper algorithm
            transformer = new DecryptTransformer(algId);
            this.crypto = crypto;
        }

        /// <summary>
        /// Takes an encrypted string and returns an unencrypted
        /// string, given the string to decrypt, the algorithm,
        ///  key and intial vector.
        /// </summary>
        /// <param name="StringToDecrypt">An encrypted string</param>
        /// <param name="key">The key used in encryption</param>
        /// <param name="IV">The Initial Vector used in encryption</param>
        /// <returns></returns>
        public string Decrypt(string StringToDecrypt, byte[] key, byte[] IV)
        {
            string roundtrippedText;
            try
            {
                //Set up the memory stream for the decrypted data.
                MemoryStream memStreamDecryptedData = new MemoryStream();
                //Create byte array from string to decrypt
                byte[] bytesCipherString = CreateByteArray(StringToDecrypt);
                byte[] bytesKey = key;
                initVec = IV;

                transformer.IV = initVec;
                ICryptoTransform transform =
                    transformer.GetCryptoServiceProvider(bytesKey);

                CryptoStream cs_tmp = new CryptoStream(memStreamDecryptedData,
                    transform, CryptoStreamMode.Write);

                CryptoStream cs_dec = new CryptoStream(cs_tmp,
                    new FromBase64Transform(), CryptoStreamMode.Write);

                cs_dec.Write(bytesCipherString, 0, bytesCipherString.Length);

                cs_dec.FlushFinalBlock();
                cs_dec.Close();

                byte[] bytesRoundtrippedText = memStreamDecryptedData.ToArray();

                // now we have our round-tripped text in a byte array
                // turn it into string for output
                roundtrippedText = System.Text.Encoding.UTF8.GetString(bytesRoundtrippedText);
            }

            catch (Exception ex)
            {
                throw new SecurityException("Error while writing encrypted data to the stream: \n"
                    + ex.Message, ex);
            }
            return roundtrippedText;
        }

        /// <summary>
        /// Creates a byte array for decryption from the input string.
        /// If the string has been formatted as hex, FormatAsHex should
        /// set to be true and the string will be converted from hex
        /// before creating the byte array. 
        /// </summary>
        /// <param name="StringToDecrypt">The encrypted string</param>
        /// <returns>A byte array from the encrypted string</returns>
        private byte[] CreateByteArray(string StringToDecrypt)
        {
            //Put the input string into the byte array.
            byte[] bytesCipherString;

            switch (crypto.StringEncoding)
            {
                case Crypto.Encoding.Encoded_As_UTF8:
                    bytesCipherString = System.Text.Encoding.UTF8.GetBytes(StringToDecrypt);
                    break;
                case Crypto.Encoding.Encoded_As_Hex:
                    bytesCipherString = new byte[StringToDecrypt.Length/2];
                    for (int x = 0; x < StringToDecrypt.Length/2; x++)
                    {
                        int i = (Convert.ToInt32(StringToDecrypt.Substring(x*2, 2), 16));
                        bytesCipherString[x] = (byte)i;
                    }
                    break;
                case Crypto.Encoding.Encoded_As_Base64:
                    bytesCipherString = Convert.FromBase64String(StringToDecrypt);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return bytesCipherString;
        }
    }

    /// <summary>
    /// Creates the proper Service Provider for the EncryptionAlgorithm
    /// </summary>
    internal class DecryptTransformer
    {
        private EncryptionAlgorithm algorithmID;
        private byte[] initVec;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="deCryptId">The EncryptionAlgorithm</param>
        internal DecryptTransformer(EncryptionAlgorithm deCryptId)
        {
            algorithmID = deCryptId;
        }

        //end GetCryptoServiceProvider
        /// <summary>
        /// The IV as an array of bytes
        /// </summary>
        internal byte[] IV
        {
            set { initVec = value; }
        }

        /// <summary>
        /// Returns the proper Service Provider. Sets the key and IV of
        /// the provider. If no key and IV were passed, a random key and/or
        /// IV are created (not very useful for decryption!).
        /// </summary>
        /// <param name="bytesKey">A byte array of the key</param>
        /// <returns>A CryptoServiceProvider for the EncryptionAlgorithm</returns>
        internal ICryptoTransform GetCryptoServiceProvider(byte[] bytesKey)
        {
            // Pick the provider.
            switch (algorithmID)
            {
                case EncryptionAlgorithm.Des:
                {
                    DES des = new DESCryptoServiceProvider();
                    des.Mode = CipherMode.CBC;
                    des.Key = bytesKey;
                    des.IV = initVec;
                    return des.CreateDecryptor();
                }
                case EncryptionAlgorithm.TripleDes:
                {
                    TripleDES des3 = new TripleDESCryptoServiceProvider();
                    des3.Mode = CipherMode.CBC;
                    return des3.CreateDecryptor(bytesKey, initVec);
                }
                case EncryptionAlgorithm.Rc2:
                {
                    RC2 rc2 = new RC2CryptoServiceProvider();
                    rc2.Mode = CipherMode.CBC;
                    return rc2.CreateDecryptor(bytesKey, initVec);
                }
                case EncryptionAlgorithm.Rijndael:
                {
                    Rijndael rijndael = new RijndaelManaged();
                    rijndael.Mode = CipherMode.CBC;
                    return rijndael.CreateDecryptor(bytesKey, initVec);
                }
                default:
                {
                    throw new CryptographicException("Algorithm ID '" +
                        algorithmID +
                            "' not supported.");
                }
            }
        }
    }
}

