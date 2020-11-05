namespace OCI
{
    public class ArtifactReference
    {
        public string Tag { get; set; }

        public string Digest { get; set; }

        public string Repository { get; set; }

        public string HostName { get; set; }
    }
}