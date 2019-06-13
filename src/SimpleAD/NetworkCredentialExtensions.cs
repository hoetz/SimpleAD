using System.Net;

namespace SimpleAD
{
    internal static class NetworkCredentialExtensions
    {
        internal static NetworkCredential EMPTY = new NetworkCredential();

        internal static string DomainAndUsername(this NetworkCredential cred)
        {
            if (!string.IsNullOrEmpty(cred.Domain))
                return string.Format("{0}\\{1}", cred.Domain, cred.UserName);
            return string.Format("{0}", cred.UserName);
        }
    }
}