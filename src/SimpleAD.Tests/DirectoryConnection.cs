using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Dynamic;
using System.Net;
using System.Security;

namespace SimpleAD.Tests
{
    public class DirectoryConnection
    {
        private NetworkCredential _credentials = NetworkCredentialExtensions.EMPTY;

        public NetworkCredential credentials
        {
            get { return this._credentials; }
        }

        private string _DomainController;

        public string DomainController
        {
            get { return this._DomainController; }
        }

        private DirectoryConnection(string domainController, NetworkCredential credentials)
        {
            this._DomainController = domainController;
            this._credentials = credentials;
        }


        public static DirectoryConnection Create()
        {
            var dc = Tests.DomainController.GetCurrent();
            return new DirectoryConnection(dc, NetworkCredentialExtensions.EMPTY);
        }

        public DirectoryConnection WithCredentials(string domain, string userName, string password)
        {
            if (string.IsNullOrEmpty(domain) || string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Domain, Username & Password required");
            }
            SecureString securePwd = new SecureString();
            foreach (char ch in password)
            {
                securePwd.AppendChar(ch);
            }
            var dc = Tests.DomainController.GetCurrent();
            return new DirectoryConnection(
                this.DomainController,
                new NetworkCredential(userName, securePwd, domain));
        }

        public DirectoryConnection WithDomainController(string domainController)
        {
            return new DirectoryConnection(domainController, this.credentials);
        }

        public IEnumerable<dynamic> Query(string ldapQuery, string searchRootPath)
        {
            DirectoryEntry searchRoot = new DirectoryEntry(searchRootPath);
            return this.Query(ldapQuery, searchRoot);
        }

        public IEnumerable<dynamic> Query(string ldapQuery)
        {
            DirectoryEntry searchRoot = this.GetSearchRoot();
            return this.Query(ldapQuery, searchRoot);
        }

        private IEnumerable<dynamic> Query(string ldapQuery, DirectoryEntry root)
        {
            DirectorySearcher search = new DirectorySearcher(root);
            search.Filter = ldapQuery;
            search.PropertiesToLoad.Add("samaccountname");
            search.PropertiesToLoad.Add("mail");
            search.PropertiesToLoad.Add("usergroup");
            search.PropertiesToLoad.Add("displayname");
            SearchResult result;
            SearchResultCollection resultCol = search.FindAll();
            if (resultCol != null)
            {
                for (int counter = 0; counter < resultCol.Count; counter++)
                {
                    dynamic exp = new ExpandoObject();
                    string UserNameEmailString = string.Empty;
                    result = resultCol[counter];
                    var entry = result.GetDirectoryEntry();
                    yield return entry.ToDynamicPropertyCollection();
                }
            }
        }

        private DirectoryEntry GetSearchRoot()
        {
            if (this.credentials != NetworkCredentialExtensions.EMPTY)
                return new DirectoryEntry(GetDefaultLDAPPath(), this.credentials.DomainAndUsername(), this.credentials.Password);
            else
                return new DirectoryEntry(GetDefaultLDAPPath());
        }

        private string GetDefaultLDAPPath()
        {
            DirectoryEntryQuery qry = new DirectoryEntryQuery(this.DomainController, this.credentials);

            DirectoryEntry ent = this.GetRootDSE();
            if (ent != null)
            {
                string DefaultNamingContext = ent.Properties["defaultNamingContext"].Value.ToString();
                if (DefaultNamingContext.Length > 0)
                {
                    return string.Format("LDAP://{0}", DefaultNamingContext);
                }
            }
            throw new InvalidOperationException("Could not find DefaultNamingContext");
        }

        private DirectoryEntry GetDefaultDomainEntry()
        {
            DirectoryEntryQuery qry = new DirectoryEntryQuery(this.DomainController, this.credentials);

            DirectoryEntry ent = this.GetRootDSE();
            if (ent != null)
            {
                string DefaultNamingContext = ent.Properties["defaultNamingContext"].Value.ToString();
                if (DefaultNamingContext.Length > 0)
                {
                    ent = qry.GetDirectoryEntry(DefaultNamingContext);
                }
                else
                {
                    ent = null;
                }
            }
            return ent;
        }

        private DirectoryEntry GetRootDSE()
        {
            string connectingPoint = "";
            if (this.DomainController.Length > 0)
            {
                connectingPoint = string.Format("LDAP://{0}/RootDSE", this.DomainController);
            }
            else
            {
                connectingPoint = "LDAP://RootDSE";
            }

            DirectoryEntry entry = null;
            if (this.credentials != NetworkCredentialExtensions.EMPTY)
            {
                try
                {
                    entry = new DirectoryEntry(connectingPoint, this.credentials.DomainAndUsername(), this.credentials.Password, AuthenticationTypes.Secure);
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
                    entry = new DirectoryEntry(connectingPoint);
                }
                catch
                {
                    entry = null;
                }
            }
            if (entry != null)
            {
                try
                {
                    string tmp = entry.Properties["defaultNamingContext"].Value.ToString();
                }
                catch
                {
                    entry = null;
                }
            }

            return entry;
        }
    }
}