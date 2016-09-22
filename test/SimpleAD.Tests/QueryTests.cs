using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Extensions;

namespace SimpleAD.Tests
{
    public class QueryTests
    {
        [Fact]
        public void Query_WithValidADConnection_ReturnsRequestedUser()
        {
            ActiveDirectory activeDirectory = ActiveDirectory.Setup();
            string sAMAccountName = "Testbenutzer";
            dynamic Results = activeDirectory.Query(string.Format("(&(objectClass=user)(objectCategory=person)(samaccountname={0}))", sAMAccountName));
            Assert.True(Results is IEnumerable);
            Assert.True(Results.First().sAMAccountName.ToLower() == sAMAccountName.ToLower());
        }

        [Fact]
        public void QueryResult_NotEmpty_ReturnsCorrectCount()
        {
            ActiveDirectory activeDirectory = ActiveDirectory.Setup();
            string sAMAccountName = "Testbenutzer";
            dynamic Results = activeDirectory.Query(string.Format("(&(objectClass=user)(objectCategory=person)(samaccountname={0}))", sAMAccountName));
            Assert.True(Results.Count()==1);
        }

        [Fact]
        public void Query_ReturnsOnlyLoadedProperties()
        {
            ActiveDirectory activeDirectory = ActiveDirectory.Setup();
            string sAMAccountName = "Testbenutzer";
            dynamic Results = activeDirectory.Query(string.Format("(&(objectClass=user)(objectCategory=person)(samaccountname={0}))", sAMAccountName), new string[] { "sAMAccountName","sn"});
            dynamic user = Results.First();
            Assert.True(user.sAMAccountName == "Testbenutzer");
            Assert.Throws<Microsoft.CSharp.RuntimeBinder.RuntimeBinderException>(() => user.givenName);
        }

        [Fact]
        public void Query_ReturnsLargeIntegerPropertyAsDateTime()
        {
            ActiveDirectory activeDirectory = ActiveDirectory.Setup();
            string sAMAccountName = "Testbenutzer";
            dynamic Results = activeDirectory.Query(string.Format("(&(objectClass=user)(objectCategory=person)(samaccountname={0}))", sAMAccountName), new string[] { "pwdLastSet" });
            dynamic user = Results.First();
            Assert.True(user.pwdLastSet is DateTime);
        }

        [Theory]
        [AutoNAttribute]
        public void Create_WithCredentials_CreatesValidConnectionCredentials(
            string domain,
            string username,
            string password)
        {
            ActiveDirectory directoryCon =
                ActiveDirectory
                .Setup()
                .WithCredentials(domain, username, password);

            Assert.True(
                directoryCon.credentials.Password == password &&
                directoryCon.credentials.Domain == domain &&
                directoryCon.credentials.UserName == username);
        }

        [Theory]
        [AutoNAttribute]
        public void Create_WithDomainController_CreatesValidConnection(
            string domainController)
        {
            ActiveDirectory directoryCon =
                ActiveDirectory
                .Setup()
                .WithDomainController(domainController);

            Assert.True(
                directoryCon.domainController.Value == domainController);
        }

        [Fact]
        public void QueryCount_NotEmpty_ReturnsNonZeroCount()
        {
            ActiveDirectory activeDirectory = ActiveDirectory.Setup();
            int Results = activeDirectory.QueryCount("(&(objectClass=user)(objectCategory=person))");
            Assert.True(Results > 0);
        }
    }
}