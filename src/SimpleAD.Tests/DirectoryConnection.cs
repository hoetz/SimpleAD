using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace SimpleAD.Tests
{
    public class DirectoryConnection
    {
        private string ldapPath;

        public DirectoryConnection(string ldapPath)
        {
            this.ldapPath = ldapPath;
        }
        public static DirectoryConnection Create(string ldapPath)
        {
            return new DirectoryConnection(ldapPath);
        }

        public IEnumerable<dynamic> Query(string ldapQuery)
        {
            DirectoryEntry searchRoot = new DirectoryEntry(this.ldapPath);
            DirectorySearcher search = new DirectorySearcher(searchRoot);
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
    }
}
