using System;
using System.Security.Cryptography;
using System.Text;
namespace PermissionManagement.Utility
{
    public sealed class PasswordHash
    {
        private PasswordHash()
        {
        }

        private static string GenerateSalt()
        {
            byte[] data = new byte[32];
            RNGCryptoServiceProvider p = new RNGCryptoServiceProvider();
            p.GetBytes(data);
            return Convert.ToBase64String(data);
        }

        private static string GenerateSalt(string saltKey)
        {
            return Convert.ToBase64String(UTF8Encoding.UTF8.GetBytes(saltKey));
        }

        private static string EncodePassword(string pass, string salt)
        {
            byte[] bytes = Encoding.Unicode.GetBytes(pass);
            byte[] src = Convert.FromBase64String(salt);
            byte[] dst = new byte[src.Length + (bytes.Length - 1) + 1];
            byte[] inArray = null;
            Buffer.BlockCopy(src, 0, dst, 0, src.Length);
            Buffer.BlockCopy(bytes, 0, dst, src.Length, bytes.Length);
            HashAlgorithm algorithm = HashAlgorithm.Create();
            inArray = algorithm.ComputeHash(dst);

            return Convert.ToBase64String(inArray);

        }

        public static string Hash(string username, string password)
        {
            string salt = GenerateSalt(username + password); // + "F!r$t8@nkN!ger!@");
            string objValue = EncodePassword(password, salt);
            if (objValue.Length > 0x80)
            {
                return null;
            }
            return objValue;
        }

    }
}
