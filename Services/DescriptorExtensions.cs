using System.Text.Json;
using System.IO;
using Microsoft.Azure.ContainerRegistry.Models;

namespace AzureContainerRegistry.CLI.Services
{
    public static class DescriptorExtensions
    {

        static JsonSerializerOptions options = new JsonSerializerOptions();

        public static Stream ToStream(this Descriptor descriptor)
        {
            byte[] jsonUtf8Bytes;
            jsonUtf8Bytes = JsonSerializer.SerializeToUtf8Bytes(descriptor, options);
            return new MemoryStream(jsonUtf8Bytes);
        }
    }
}