using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        private DomainController _domainController = DomainController.NONE;

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
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Username & Password required");
            }
            SecureString securePwd = new SecureString();
            foreach (char ch in password)
            {
                securePwd.AppendChar(ch);
            }
            var cred = new NetworkCredential(userName, securePwd, domain);
            if (string.IsNullOrEmpty(domain))
            {
                cred = new NetworkCredential(userName, securePwd);
            }
            return new ActiveDirectory(
                this.domainController,
                cred);
        }

        public ActiveDirectory WithDomainController(string domainController)
        {
            if (string.IsNullOrEmpty(domainController))
                throw new ArgumentException("DomainController must not be empty");
            return new ActiveDirectory(new DomainController(domainController), this.credentials);
        }

        public int QueryCount(string ldapQuery, string searchRootDN = null)
        {
            DirectoryEntry searchRoot = null;
            if (string.IsNullOrEmpty(searchRootDN) == false)
                searchRoot = new DirectoryEntry(string.Format("LDAP://{0}{1}", InsertDCInLDAPPathIfAvailable(), searchRootDN));
            else
                searchRoot = GetSearchRoot();

            using (DirectorySearcher srch = new DirectorySearcher(searchRoot, ldapQuery))
            {
                srch.PageSize = 1000;
                try
                {
                    return srch.FindAll().Count;
                }
                catch
                {
                    throw new ArgumentException("LDAP Query or searchRoot invalid");
                }
            }
        }


        public QueryResult Query(string ldapQuery, string searchRootPath, IEnumerable<string> propertiesToLoad = null)
        {
            DirectoryEntry searchRoot = new DirectoryEntry(searchRootPath);
            try
            {
                string testSearchRootName = searchRoot.Name;
            }
            catch
            {
                throw new ArgumentException("Invalid Search Root Path");
            }
            return this.Query(ldapQuery, searchRoot, propertiesToLoad);
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
                var dynCollection = new DynamicActiveDirectoryObject(entry);
                try
                {
                    dynCollection = entry.ToDynamicPropertyCollection(propertiesToLoad);
                }
                catch
                {
                    Debug.WriteLine($"WARNING: Could not bind {entry.Path}");
                    continue;
                }
                yield return dynCollection;
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
                    return string.Format("LDAP://{0}{1}", InsertDCInLDAPPathIfAvailable(), DefaultNamingContext);
                }
            }
            throw new InvalidOperationException("Could not find DefaultNamingContext, check your credentials");
        }

        private string InsertDCInLDAPPathIfAvailable()
        {
            if (this.domainController != DomainController.NONE)
            {
                return string.Format("{0}/", this.domainController.Value);
            }
            else
                return string.Empty;
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