using System;
using System.Security.Cryptography;
using System.Text;

// Security: Hệ thống mã hoá AES-256 bit giúp chống hack file save game.
public static class Security
{
    // Key và IV phải được giữ bí mật. Key 32 bytes = 256 bits.
    private static readonly string key = "FarmingGame123456789012345678901"; 
    private static readonly string iv  = "InitVector123456"; 

    public static string Encrypt(string plainText)
    {
        byte[] encrypted;
        using (Aes aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes(key);
            aes.IV = Encoding.UTF8.GetBytes(iv);
            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using (var ms = new System.IO.MemoryStream())
            {
                using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                {
                    using (var sw = new System.IO.StreamWriter(cs))
                    {
                        sw.Write(plainText);
                    }
                    encrypted = ms.ToArray();
                }
            }
        }
        return Convert.ToBase64String(encrypted);
    }

    public static string Decrypt(string cipherText)
    {
        string plaintext = null;
        byte[] cipherBytes = Convert.FromBase64String(cipherText);
        using (Aes aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes(key);
            aes.IV = Encoding.UTF8.GetBytes(iv);
            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using (var ms = new System.IO.MemoryStream(cipherBytes))
            {
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                {
                    using (var sr = new System.IO.StreamReader(cs))
                    {
                        plaintext = sr.ReadToEnd();
                    }
                }
            }
        }
        return plaintext;
    }
}
