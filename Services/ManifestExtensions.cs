using System.Collections.Generic;
using Microsoft.Azure.ContainerRegistry.Models;
using OCI;

namespace AzureContainerRegistry.CLI.Services
{
    public static class ManifestHelpers
    {
        public static Manifest Convert(this ManifestWrapper manifestResponse, string mediaType)
        {
            Manifest manifest = null;
            switch (mediaType)
            {
                case ManifestMediaTypes.V2Manifest:
                    manifest = (V2Manifest)manifestResponse;
                    break;
                case ManifestMediaTypes.V1Manifest:
                    manifest = (V1Manifest)manifestResponse;
                    break;
                case ManifestMediaTypes.ManifestList:
                    manifest = (ManifestList)manifestResponse;
                    break;
                case ManifestMediaTypes.OCIIndex:
                    manifest = (OCIIndex)manifestResponse;
                    break;
                case ManifestMediaTypes.OCIManifest:
                    manifest = (OCIManifest)manifestResponse;
                    break;
            }

            return manifest;
        }


        public static IList<Descriptor> Layers(this Manifest manifest, string mediaType)
        {
            switch (manifest)
            {
                case OCIManifest oci:
                    return oci.Layers;
                case V2Manifest v2m:
                    return v2m.Layers;
            }

            return new List<Descriptor>();
        }

        public static Descriptor Config(this Manifest manifest, string mediaType)
        {
            switch (manifest)
            {
                case OCIManifest oci:
                    return oci.Config;
                case V2Manifest v2m:
                    return v2m.Config;
            }

            return null;
        }
    }
}