using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Xunit.Extensions;

namespace SimpleAD.Tests
{
    public class DirectoryConnectionTests
    {
        [Fact]
        public void Query_WithValidADConnection_ReturnsRequestedUser()
        {
            DirectoryConnection directoryCon = DirectoryConnection.Create("LDAP://DC=gab,DC=loc");
            string sAMAccountName = "florian.hoetzinger";
            dynamic Results = directoryCon.Query(string.Format("(&(objectClass=user)(objectCategory=person)(samaccountname={0}))", sAMAccountName));
            Assert.True(Results is IEnumerable);
            Assert.True(Enumerable.First(Results).sAMAccountName.ToLower() == sAMAccountName);
        }
    }
}
