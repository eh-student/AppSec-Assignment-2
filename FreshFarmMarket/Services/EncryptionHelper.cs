using System.Security.Cryptography;

namespace FreshFarmMarket.Services
{
    public static class EncryptionHelper
    {
        public static (string key, string iv) GenerateEncryptionKeys()
        {
            using (Aes aes = Aes.Create())
            {
                aes.GenerateKey();
                aes.GenerateIV();

                string key = Convert.ToBase64String(aes.Key); // 32 bytes = 256 bits
                string iv = Convert.ToBase64String(aes.IV);   // 16 bytes = 128 bits

                return (key, iv);
            }
        }
    }
}
