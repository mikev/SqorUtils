using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Sqor.Utils.Encryption
{
    public static class SHA256Hash
    {
        public static string Create(string input)
        {
            var sha = SHA256.Create();
            var inputBytes = Encoding.ASCII.GetBytes(input);
            var hashBytes = sha.ComputeHash(inputBytes);
            var result = string.Join("", hashBytes.Select(x => x.ToString("X2")));
            return result;
        }
    }
}