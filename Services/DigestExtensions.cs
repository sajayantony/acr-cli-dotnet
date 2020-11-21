
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Azure.ContainerRegistry.Models;

namespace AzureContainerRegistry.CLI.Services
{
    static class DigestExtensions 
    {
        public static string ComputeHash(this Stream stream)
        {
            using (SHA256 mySHA256 = SHA256.Create())
            {
                // Compute and print the hash values for each file in directory.
                try
                {
                    var position = stream.Position;
                    stream.Position = 0;
                    var hashValue = mySHA256.ComputeHash(stream);
                    stream.Position = position;
                    return "sha256:" + PrintByteArray(hashValue);
                }
                catch (IOException e)
                {
                    Console.WriteLine($"I/O Exception: {e.Message}");
                    throw;
                }
                catch (UnauthorizedAccessException e)
                {
                    Console.WriteLine($"Access Exception: {e.Message}");
                    throw;
                }
            }
        }        

        // Display the byte array in a readable format.
        private static string PrintByteArray(byte[] array)
        {
            var builder = new StringBuilder();
            for (int i = 0; i < array.Length; i++)
            {
                builder.AppendFormat($"{array[i]:X2}");
                if ((i % 4) == 3) Console.Write(" ");
            }
            return builder.ToString().ToLower();
        }         
    }
}