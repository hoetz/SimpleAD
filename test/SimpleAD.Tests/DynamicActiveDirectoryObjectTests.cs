using System;
using System.Collections;
using System.Linq;
using Xunit;
using Xunit.Extensions;

namespace SimpleAD.Tests
{
    public class DynamicActiveDirectoryObjectTests
    {
        [Theory]
        [AutoN]
        public void Save_ValidUser_WritesPropertiesCorrectly(string wWWHomePage)
        {
            string sAMAccountName = "Testbenutzer";
            ActiveDirectory activeDirectory = ActiveDirectory.Setup();

            string ldapQuery=string.Format("(&(objectClass=user)(objectCategory=person)(samaccountname={0}))", sAMAccountName);
            string[] propsToLoad=new string[] { "sAMAccountName", "wWWHomePage" };

            dynamic Results = activeDirectory.Query(ldapQuery,propsToLoad);
            dynamic user = Results.First();

            user.wWWHomePage = wWWHomePage;
            user.Save();

            Results = activeDirectory.Query(ldapQuery, propsToLoad);
            var actual = Results.First();
            Assert.True(actual["wWWHomePage"] == wWWHomePage);
        }
    }
}
