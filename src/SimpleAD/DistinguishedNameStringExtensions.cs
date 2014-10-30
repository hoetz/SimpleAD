using System;
using System.Text;

namespace SimpleAD
{
    internal static class DistinguishedNameStringExtensions
    {
        public static string ReplaceDNSpecialChars(this string UserDN)
        {
            StringBuilder stb = new StringBuilder();
            string RetVal = UserDN;
            char[] specialChars = new char[] { '/', '+', '<', '>', '#', ';', '"', ',' };
            RetVal = RetVal.Replace("\\", "");

            foreach (char chr in specialChars)
            {
                RetVal = RetVal.Replace(chr.ToString(), string.Format("\\{0}", chr));
            }
            return (RetVal);
        }

        public static string DNtoDNSDomain(this string strDN)
        {
            string strDNSDomain = "";
            int pos = strDN.ToLower().IndexOf("dc=");
            if (pos >= 0)
            {
                strDNSDomain = strDN.Substring(pos + 3).ToLower().Replace(",dc=", ".");
            }
            return (strDNSDomain);
        }

        public static string MaskUserDN(this string distinguishedName)
        {
            StringBuilder stb = new StringBuilder();
            string[] arDN = distinguishedName.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string DN in arDN)
            {
                if (DN.ToLower().StartsWith("cn=") || DN.ToLower().StartsWith("ou=") || DN.ToLower().StartsWith("dc="))
                {
                    stb.Append(string.Format(",{0}{1}", DN.Substring(0, 3), DN.Substring(3).ReplaceDNSpecialChars()));
                }
                else
                {
                    stb.Append(string.Format("\\,{0}", DN.ReplaceDNSpecialChars()));
                }
            }
            if (stb.Length > 0)
            {
                return (stb.ToString().Substring(1));
            }
            else
            {
                return (distinguishedName);
            }
        }
    }
}