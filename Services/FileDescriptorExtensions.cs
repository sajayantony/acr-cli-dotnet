
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Azure.ContainerRegistry.Models;

namespace OCI
{
    static class FileDescriptorExtensions 
    {
        public static void Add(this V2Manifest manifest, FileInfo fileInfo) => manifest.Layers.Add(fileInfo.ToDescriptor());

        public static Descriptor ToDescriptor(this FileInfo fInfo)
        {
            var descriptor = new Descriptor();
            
            descriptor.Size = fInfo.Length;
            descriptor.Digest = fInfo.ComputeHash();            
            return descriptor;
        }

        static string ComputeHash(this FileInfo fInfo)
        {
            using (SHA256 mySHA256 = SHA256.Create())
            {
                // Compute and print the hash values for each file in directory.
                try
                {
                    FileStream fileStream = fInfo.OpenRead();
                    fileStream.Position = 0;
                    
                    // Compute the hash of the fileStream.
                    byte[] hashValue = mySHA256.ComputeHash(fileStream);
                    
                    // Write the name and hash value of the file to the console.
                    Console.Write($"{fInfo.Name}: ");
                    //PrintByteArray(hashValue);
                    fileStream.Close();

                     //return "sha256:" + BitConverter.ToString(hashValue);
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
        public static string PrintByteArray(byte[] array)
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