
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Azure.ContainerRegistry.Models;

namespace AzureContainerRegistry.CLI.Services
{
    static class FileDescriptorExtensions
    {
        public static Descriptor ToDescriptor(this FileInfo fInfo)
        {

            //Default media type for file is "application/vnd.docker.image.rootfs.diff.tar.gzip"
            return new Descriptor()
            {
                Size = fInfo.Length,
                Digest = fInfo.ComputeHash(), 
                MediaType = "application/vnd.docker.image.rootfs.diff.tar.gzip"

            };
        }

        static string ComputeHash(this FileInfo fInfo)
        {
            using (SHA256 mySHA256 = SHA256.Create())
            {
                // Compute and print the hash values for each file in directory.
                try
                {
                    using (FileStream fileStream = fInfo.OpenRead())
                    {
                        fileStream.Position = 0;
                        return fileStream.ComputeHash();
                    }
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
    }
}