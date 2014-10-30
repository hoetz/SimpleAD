using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Dynamic;
using System.Net;
using System.Security;

namespace SimpleAD
{
    public class ActiveDirectory
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

        private ActiveDirectory(DomainController domainController, NetworkCredential credentials)
        {
            this._domainController = domainController;
            this._credentials = credentials;
        }

        /// <summary>
        /// Initializes a new <see cref="ActiveDirectory"/> instance without a DomainController or Credentials
        /// </summary>
        /// <returns></returns>
        public static ActiveDirectory Setup()
        {
            var dc = DomainController.GetCurrent();
            return new ActiveDirectory(DomainController.NONE, NetworkCredentialExtensions.EMPTY);
        }

        public ActiveDirectory WithCredentials(string domain, string userName, string password)
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
            var dc = DomainController.GetCurrent();
            return new ActiveDirectory(
                this.domainController,
                new NetworkCredential(userName, securePwd, domain));
        }

        public ActiveDirectory WithDomainController(string domainController)
        {
            if (string.IsNullOrEmpty(domainController))
                throw new ArgumentException("DomainController must not be empty");
            return new ActiveDirectory(new DomainController(domainController), this.credentials);
        }

        public QueryResult Query(string ldapQuery, string searchRootPath)
        {
            DirectoryEntry searchRoot = new DirectoryEntry(searchRootPath);
            return this.Query(ldapQuery, searchRoot);
        }

        public QueryResult Query(string ldapQuery)
        {
            DirectoryEntry searchRoot = this.GetSearchRoot();
            return this.Query(ldapQuery, searchRoot);
        }

        private QueryResult Query(string ldapQuery, DirectoryEntry root)
        {
            DirectorySearcher search = new DirectorySearcher(root);
            search.Filter = ldapQuery;
            search.PropertiesToLoad.Add("samaccountname");
            search.PropertiesToLoad.Add("mail");
            search.PropertiesToLoad.Add("usergroup");
            search.PropertiesToLoad.Add("displayname");
            SearchResultCollection resultCol = search.FindAll();
            if (resultCol != null)
            {
                return new QueryResult(this.ProcessSearchResults(resultCol));
            }
            return new QueryResult();
        }

        private IEnumerable<dynamic> ProcessSearchResults(SearchResultCollection srCol)
        {
            for (int counter = 0; counter < srCol.Count; counter++)
            {
                string UserNameEmailString = string.Empty;
                var result = srCol[counter];
                var entry = result.GetDirectoryEntry();
                yield return entry.ToDynamicPropertyCollection();
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