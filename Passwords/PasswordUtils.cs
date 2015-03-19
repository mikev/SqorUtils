#if FULL
namespace Sqor.Utils.Passwords
{
    public class PasswordUtils
    {
        public static string CreateHash(string password)
        {
            var salt = BCrypt.Net.BCrypt.GenerateSalt(5);
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password, salt);
            return hashedPassword;
        }

        public static bool VerifyPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
    }
}
#endif
