namespace OCI
{
    public class ImageReference
    {
        public string Tag { get; set; }

        public string Digest { get; set; }


        public string Repository { get; set; }

        public string HostName { get ; set;}
    }


    static class ManifestMediaTypes 
    {
        public static readonly string V2Manifest = "application/vnd.docker.distribution.manifest.v2+json";
        public static readonly string V1Manifest = "application/vnd.docker.container.image.v1+json";
        public static readonly string OCIManifest = "application/vnd.oci.image.manifest.v1+json";
        public static readonly string OCIIndex = "application/vnd.oci.image.index.v1+json";
        public static readonly string ManifestList = "application/vnd.docker.distribution.manifest.list.v2+json";
    }
}