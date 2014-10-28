using System.Collections;
using System.Linq;
using Xunit;
using Xunit.Extensions;

namespace SimpleAD.Tests
{
    public class DirectoryConnectionTests
    {
        [Fact]
        public void Query_WithValidADConnection_ReturnsRequestedUser()
        {
            DirectoryConnection directoryCon = DirectoryConnection.Create();
            string sAMAccountName = "florian.hoetzinger";
            dynamic Results = directoryCon.Query(string.Format("(&(objectClass=user)(objectCategory=person)(samaccountname={0}))", sAMAccountName));
            Assert.True(Results is IEnumerable);
            Assert.True(Enumerable.First(Results).sAMAccountName.ToLower() == sAMAccountName);
        }

        [Theory]
        [AutoNAttribute]
        public void Create_WithCredentials_CreatesValidConnectionCredentials(
            string domain,
            string username,
            string password)
        {
            DirectoryConnection directoryCon =
                DirectoryConnection
                .Create()
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
            DirectoryConnection directoryCon =
                DirectoryConnection
                .Create()
                .WithDomainController(domainController);

            Assert.True(
                directoryCon.DomainController == domainController);
        }
    }
}