using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace IronVault
{
    public static class CryptoEngine
    {
        private static readonly int SaltSize = 16;
        private static readonly int Iterations = 100000;
        private static readonly int BufferSize = 1024 * 1024;

        public static async Task EncryptFileAsync(string inputFile, string outputFile, string password, IProgress<double> progress)
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

                    using (FileStream fsOut = new FileStream(outputFile, FileMode.Create, FileAccess.Write, FileShare.None, BufferSize, true))
                    {
                        await fsOut.WriteAsync(salt, 0, salt.Length);
                        await fsOut.WriteAsync(iv, 0, iv.Length);

                        using (CryptoStream cs = new CryptoStream(fsOut, aes.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            using (FileStream fsIn = new FileStream(inputFile, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize, true))
                            {
                                await CopyStreamWithProgressAsync(fsIn, cs, fsIn.Length, progress);
                            }
                        }
                    }
                }
            }
        }

        public static async Task DecryptFileAsync(string inputFile, string outputFile, string password, IProgress<double> progress)
        {
            using (FileStream fsIn = new FileStream(inputFile, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize, true))
            {
                byte[] salt = new byte[SaltSize];
                await fsIn.ReadAsync(salt, 0, salt.Length);

                using (Aes aes = Aes.Create())
                {
                    byte[] iv = new byte[aes.BlockSize / 8];
                    await fsIn.ReadAsync(iv, 0, iv.Length);
                    aes.IV = iv;

                    using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
                    {
                        aes.Key = deriveBytes.GetBytes(32);

                        using (CryptoStream cs = new CryptoStream(fsIn, aes.CreateDecryptor(), CryptoStreamMode.Read))
                        {
                            using (FileStream fsOut = new FileStream(outputFile, FileMode.Create, FileAccess.Write, FileShare.None, BufferSize, true))
                            {
                                await CopyStreamWithProgressAsync(cs, fsOut, fsIn.Length, progress);
                            }
                        }
                    }
                }
            }
        }

        private static async Task CopyStreamWithProgressAsync(Stream source, Stream destination, long totalLength, IProgress<double> progress)
        {
            byte[] buffer = new byte[BufferSize];
            long totalRead = 0;
            int bytesRead;

            while ((bytesRead = await source.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await destination.WriteAsync(buffer, 0, bytesRead);
                totalRead += bytesRead;

                if (totalLength > 0 && progress != null)
                {
                    progress.Report((double)totalRead / totalLength * 100);
                }
            }
        }
    }
}
