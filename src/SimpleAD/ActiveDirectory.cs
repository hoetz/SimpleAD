using System;
using System.Collections.Generic;
using System.DirectoryServices;
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
            return new ActiveDirectory(
                DomainController.NONE,
                new NetworkCredential(userName, securePwd, domain));
        }

        public ActiveDirectory WithDomainController(string domainController)
        {
            if (string.IsNullOrEmpty(domainController))
                throw new ArgumentException("DomainController must not be empty");
            return new ActiveDirectory(new DomainController(domainController), this.credentials);
        }

        public QueryResult Query(string ldapQuery, string searchRootPath, IEnumerable<string> propertiesToLoad=null)
        {
            DirectoryEntry searchRoot = new DirectoryEntry(searchRootPath);
            return this.Query(ldapQuery, searchRoot,propertiesToLoad);
        }

        /// <summary>
        /// Executes a search in the configured Active Directory connection.
        /// </summary>
        /// <param name="ldapQuery">The LDAP search filter</param>
        /// <param name="propertiesToLoad">The list of properties to load. Default: All properties are loaded.</param>
        /// <returns></returns>
        public QueryResult Query(string ldapQuery, IEnumerable<string> propertiesToLoad = null)
        {
            DirectoryEntry searchRoot = this.GetSearchRoot();
            return this.Query(ldapQuery, searchRoot, propertiesToLoad);
        }

        private QueryResult Query(string ldapQuery, DirectoryEntry root, IEnumerable<string> propertiesToLoad)
        {
            using (DirectorySearcher search = new DirectorySearcher(root))
            {
                search.Filter = ldapQuery;
                search.PageSize = 1000;

                if (propertiesToLoad != null)
                {
                    foreach (var prop in propertiesToLoad)
                    {
                        search.PropertiesToLoad.Add(prop);
                    }
                }

                SearchResultCollection resultCol = search.FindAll();
                if (resultCol != null)
                {
                    return new QueryResult(this.ProcessSearchResults(resultCol, propertiesToLoad));
                }
                return new QueryResult();
            }
        }

        private IEnumerable<dynamic> ProcessSearchResults(SearchResultCollection srCol, IEnumerable<string> propertiesToLoad)
        {
            foreach (SearchResult item in srCol)
            {
                var entry = item.GetDirectoryEntry();
                yield return entry.ToDynamicPropertyCollection(propertiesToLoad);
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
            throw new InvalidOperationException("Could not find DefaultNamingContext, check your credentials");
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