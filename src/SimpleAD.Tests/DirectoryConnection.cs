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

        private DomainController _domainController;

        public DomainController domainController
        {
            get { return this._domainController; }
        }

        private DirectoryConnection(DomainController domainController, NetworkCredential credentials)
        {
            this._domainController = domainController;
            this._credentials = credentials;
        }

        public static DirectoryConnection Create()
        {
            var dc = Tests.DomainController.GetCurrent();
            return new DirectoryConnection(DomainController.NONE, NetworkCredentialExtensions.EMPTY);
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
                this.domainController,
                new NetworkCredential(userName, securePwd, domain));
        }

        public DirectoryConnection WithDomainController(string domainController)
        {
            if (string.IsNullOrEmpty(domainController))
                throw new ArgumentException("DomainController must not be empty");
            return new DirectoryConnection(new DomainController(domainController), this.credentials);
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

        private DirectoryEntry GetRootDSE()
        {
            string connectingPoint = "";
            if (this.domainController != DomainController.NONE)
            {
                connectingPoint = string.Format("LDAP://{0}/RootDSE", this.domainController.Value);
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