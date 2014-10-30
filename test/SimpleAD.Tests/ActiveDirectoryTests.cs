using System.Collections;
using System.Linq;
using Xunit;
using Xunit.Extensions;

namespace SimpleAD.Tests
{
    public class ActiveDirectoryTests
    {
        [Fact]
        public void Query_WithValidADConnection_ReturnsRequestedUser()
        {
            ActiveDirectory activeDirectory = ActiveDirectory.Setup();
            string sAMAccountName = "florian.hoetzinger";
            dynamic Results = activeDirectory.Query(string.Format("(&(objectClass=user)(objectCategory=person)(samaccountname={0}))", sAMAccountName));
            Assert.True(Results is IEnumerable);
            Assert.True(Results.First().sAMAccountName.ToLower() == sAMAccountName);
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
    }
}