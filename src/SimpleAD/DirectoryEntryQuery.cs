using System;
using System.DirectoryServices;
using System.Net;
using System.Text;

namespace SimpleAD
{
    internal class DirectoryEntryQuery
    {
        private string domainController;
        private NetworkCredential cred;

        internal DirectoryEntryQuery(string domainController, NetworkCredential cred)
        {
            this.domainController = domainController;
            this.cred = cred;
        }

        internal DirectoryEntry GetDirectoryEntry(string distinguishedName)
        {
            return GetDirectoryEntry(distinguishedName, false);
        }

        internal DirectoryEntry GetDirectoryEntry(string distinguishedName, bool UseGC)
        {
            bool ConnectDomain = false;
            DirectoryEntry entry = null;
            distinguishedName = FormatDirectoryDistinguishedName(distinguishedName, this.domainController, BindingMethod.distinguishedName, UseGC, out ConnectDomain);
            if (distinguishedName.Length > 0)
            {
                entry = GetDirectoryEntryRaw(distinguishedName);
                if (entry != null)
                {
                    if (ConnectDomain == false && UseGC == false)
                    {
                        try
                        {
                            entry.RefreshCache(new string[] { "distinguishedName" });
                            string tmp = entry.Properties["distinguishedName"].Value.ToString();
                        }
                        catch
                        {
                            entry = null;
                        }
                    }
                    else
                    {
                        try
                        {
                            string tmp = entry.Path;
                        }
                        catch
                        {
                            entry = null;
                        }
                    }
                }
            }
            return (entry);
        }

        private DirectoryEntry GetDirectoryEntryRaw(string connectString)
        {
            DirectoryEntry entry = null;
            if (this.cred != null && !string.IsNullOrEmpty(this.cred.DomainAndUsername()))
            {
                try
                {
                    entry = new DirectoryEntry(connectString, this.cred.DomainAndUsername(), this.cred.Password, AuthenticationTypes.Secure);
                }
                catch
                {
                    entry = null;
                }
            }
            else
            {
                try
                {
                    entry = new DirectoryEntry(connectString);
                }
                catch
                {
                    entry = null;
                }
            }
            return entry;
        }

        internal static string FormatDirectoryDistinguishedName(string distinguishedName, string DomainController, BindingMethod bindingMethod, bool useGC, out bool ConnectDomain)
        {
            int pos = 0, pos2 = 0;

            string Protocol = "LDAP://";
            ConnectDomain = false;
            switch (bindingMethod)
            {
                #region bindingmethod.guid

                case BindingMethod.Guid:
                    {
                        if (useGC == true)
                        {
                            Protocol = "GC://";
                        }

                        if (DomainController.Length > 0 && useGC == false)
                        {
                            //distinguishedName = string.Format("{0}{1}/<GUID={2}>", Protocol, this.connection.DomainController, this.BigEndianGUIDToLittleEndianGUID(distinguishedName));
                            distinguishedName = string.Format("{0}{1}/<GUID={2}>", Protocol, DomainController, distinguishedName);
                        }
                        else
                        {
                            //distinguishedName = string.Format("{0}<GUID={1}>", Protocol, this.BigEndianGUIDToLittleEndianGUID(distinguishedName));
                            distinguishedName = string.Format("{0}<GUID={1}>", Protocol, distinguishedName);
                        }
                        break;
                    }

                #endregion bindingmethod.guid

                #region bindingmethod.distinguishedname

                case BindingMethod.distinguishedName:
                    {
                        pos2 = distinguishedName.IndexOf("://");
                        if (pos2 > 0)
                        {
                            distinguishedName = distinguishedName.Substring(pos2 + 3);
                        }
                        distinguishedName = distinguishedName.MaskUserDN();

                        if (useGC == true)
                        {
                            DomainController = "";
                            Protocol = "GC://";
                        }

                        if (distinguishedName.ToLower().StartsWith("dc=") == true && distinguishedName.IndexOf("/") < 0)
                        {
                            if (useGC == false)
                            {
                                distinguishedName = distinguishedName.DNtoDNSDomain();
                            }
                            ConnectDomain = true;
                        }
                        else
                        {
                            pos = distinguishedName.ToLower().IndexOf("cn=");
                            if (pos < 0)
                            {
                                pos = distinguishedName.ToLower().IndexOf("ou=");
                                if (pos < 0)
                                {
                                    pos = distinguishedName.ToLower().IndexOf("dc=");
                                    if (pos < 0)
                                        pos = 0;
                                }
                            }
                        }

                        if (DomainController.Length == 0)
                        {
                            distinguishedName = string.Format("{0}{1}", Protocol, distinguishedName.Substring(pos));
                        }
                        else
                        {
                            if (ConnectDomain == true)
                            {
                                if (useGC == true)
                                {
                                    distinguishedName = string.Format("{0}{1}", Protocol, distinguishedName.Substring(pos));
                                }
                                else
                                {
                                    distinguishedName = string.Format("{0}{1}", Protocol, DomainController);
                                }
                            }
                            else
                            {
                                if (distinguishedName.ToLower().IndexOf("rootdse") > 0) // z. B. RootDSE
                                {
                                    distinguishedName = string.Format("{0}{1}", Protocol, distinguishedName.Substring(pos));
                                }
                                else
                                {
                                    distinguishedName = string.Format("{0}{1}/{2}", Protocol, DomainController, distinguishedName.Substring(pos));
                                }
                            }
                        }
                        break;
                    }

                #endregion bindingmethod.distinguishedname

                #region bindingmethod.distinguishedname

                case BindingMethod.SID:
                    {
                        if (DomainController.Length == 0)
                        {
                            distinguishedName = string.Format("LDAP:/<SID={0}>", distinguishedName);
                        }
                        else
                        {
                            distinguishedName = string.Format("LDAP://{0}/<SID={1}>", DomainController, distinguishedName.Substring(pos));
                        }
                        break;
                    }

                #endregion bindingmethod.distinguishedname
            }
            return distinguishedName;
        }
    }

}