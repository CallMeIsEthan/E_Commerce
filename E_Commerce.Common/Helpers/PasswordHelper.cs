using Microsoft.AspNet.Identity;

namespace E_Commerce.Common.Helpers
{
    public static class PasswordHelper
    {
        private static readonly PasswordHasher Hasher = new PasswordHasher();

        public static string HashPassword(string password)
        {
            return Hasher.HashPassword(password);
        }

        public static bool VerifyPassword(string hashedPassword, string password)
        {
            var result = Hasher.VerifyHashedPassword(hashedPassword, password);
            return result == PasswordVerificationResult.Success;
        }
    }
}