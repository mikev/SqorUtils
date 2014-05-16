using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Sqor.Utils.Encryption
{
    public class HmacSha1
    {
        public static byte[] Create(string input, string key)
        {
            var sha = new HMACSHA1(Encoding.UTF8.GetBytes(key));
            var inputBytes = Encoding.UTF8.GetBytes(input);
            var hashBytes = sha.ComputeHash(inputBytes);
            return hashBytes;
        }
    }
}