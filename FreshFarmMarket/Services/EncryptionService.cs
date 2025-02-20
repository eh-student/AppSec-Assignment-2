using System.Security.Cryptography;

namespace FreshFarmMarket.Services
{
    public interface IEncryptionService
    {
        string EncryptData(string data);
        string DecryptData(string encryptedData);
    }

    public class EncryptionService : IEncryptionService
    {
        private readonly IConfiguration _configuration;
        private readonly byte[] _key;
        private readonly byte[] _iv;

        public EncryptionService(IConfiguration configuration)
        {
            _configuration = configuration;
            // Get from secure configuration
            _key = Convert.FromBase64String(_configuration["Encryption:Key"]);
            _iv = Convert.FromBase64String(_configuration["Encryption:IV"]);
        }

        public string EncryptData(string data)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = _key;
                aes.IV = _iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(data);
                    }

                    return Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
        }

        public string DecryptData(string encryptedData)
        {
            byte[] cipherText = Convert.FromBase64String(encryptedData);

            using (Aes aes = Aes.Create())
            {
                aes.Key = _key;
                aes.IV = _iv;

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                {
                    return srDecrypt.ReadToEnd();
                }
            }
        }
    }
}
