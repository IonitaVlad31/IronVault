using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace IronVault
{
    public static class CryptoEngine
    {
        private static readonly int SaltSize = 16;
        private static readonly int Iterations = 100000;

        public static void EncryptFile(string inputFile, string outputFile, string password)
        {
            byte[] salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
            {
                byte[] key = deriveBytes.GetBytes(32);

                using (Aes aes = Aes.Create())
                {
                    aes.Key = key;
                    aes.GenerateIV();
                    byte[] iv = aes.IV;

                    using (FileStream fsOut = new FileStream(outputFile, FileMode.Create))
                    {
                        fsOut.Write(salt, 0, salt.Length);
                        fsOut.Write(iv, 0, iv.Length);

                        using (CryptoStream cs = new CryptoStream(fsOut, aes.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            using (FileStream fsIn = new FileStream(inputFile, FileMode.Open))
                            {
                                fsIn.CopyTo(cs);
                            }
                        }
                    }
                }
            }
        }

        public static void DecryptFile(string inputFile, string outputFile, string password)
        {
            using (FileStream fsIn = new FileStream(inputFile, FileMode.Open))
            {
                byte[] salt = new byte[SaltSize];
                fsIn.Read(salt, 0, salt.Length);

                using (Aes aes = Aes.Create())
                {
                    byte[] iv = new byte[aes.BlockSize / 8];
                    fsIn.Read(iv, 0, iv.Length);
                    aes.IV = iv;

                    using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
                    {
                        aes.Key = deriveBytes.GetBytes(32);

                        using (CryptoStream cs = new CryptoStream(fsIn, aes.CreateDecryptor(), CryptoStreamMode.Read))
                        {
                            using (FileStream fsOut = new FileStream(outputFile, FileMode.Create))
                            {
                                cs.CopyTo(fsOut);
                            }
                        }
                    }
                }
            }
        }
    }
}
