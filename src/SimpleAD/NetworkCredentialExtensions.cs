using System.Net;

namespace SimpleAD
{
    public static class NetworkCredentialExtensions
    {
        public static NetworkCredential EMPTY = new NetworkCredential();

        public static string DomainAndUsername(this NetworkCredential cred)
        {
            return string.Format("{0}\\{1}", cred.Domain, cred.UserName);
        }
    }
}