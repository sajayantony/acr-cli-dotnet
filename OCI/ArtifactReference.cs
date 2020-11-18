using System;

namespace OCI
{

#nullable disable
    public class ArtifactReference
    {
        public string Tag { get; set; }

        public string Digest { get; set; }

        public string Repository { get; set; }

        public string HostName { get; set; }

        public override string ToString()
        {
            if (!String.IsNullOrEmpty(this.Tag))
            {
                return $"{this.HostName}/{this.Repository}:{this.Tag}";
            }
            else
            {
                return $"{this.HostName}/{this.Repository}:{this.Digest}";
            }
        }
    }
}