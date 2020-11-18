using System.Text.RegularExpressions;

namespace OCI.RegularExpressions
{
    // Refere here for base implementation https://github.com/docker/distribution/blob/master/reference/regexp.go
    public static class Regexp
    {
        const string TagPattern = @"^[\w][\w.-]{0,127}$";

        static Regex _tagExp = new System.Text.RegularExpressions.Regex(TagPattern, RegexOptions.Compiled);

        const string DigestPattern = @"[A-Za-z][A-Za-z0-9]*(?:[-_+.][A-Za-z][A-Za-z0-9]*)*[:][[:xdigit:]]{32,}";

        static Regex _digestExp = new Regex(DigestPattern, RegexOptions.Compiled);

        const string DomainPattern = @"(?:[a-zA-Z0-9]|[a-zA-Z0-9][a-zA-Z0-9-]*[a-zA-Z0-9])";

        static Regex _domainExp = new Regex(DomainPattern, RegexOptions.Compiled);

        public static bool IsValidTag(string tagPart)
        {
            return _tagExp.IsMatch(tagPart);            
        }

        public static bool IsValidDigest(string digestPart)
        {
            return _digestExp.IsMatch(digestPart);
        }

        public static bool IsValidHostName(string domainPart)
        {
            return _domainExp.IsMatch(domainPart);
        }
    }
}