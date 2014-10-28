using System.DirectoryServices.ActiveDirectory;

namespace SimpleAD.Tests
{
    public static class DomainController
    {
        public static string GetCurrent()
        {
            string SiteName = string.Empty;
            try
            {
                SiteName = ActiveDirectorySite.GetComputerSite().Name;
            }
            catch
            { }
            Domain currentDomain = Domain.GetCurrentDomain();
            if (string.IsNullOrEmpty(SiteName))
            {
                return currentDomain.FindDomainController(LocatorOptions.WriteableRequired).Name;
            }
            else
            {
                return currentDomain.FindDomainController(SiteName, LocatorOptions.WriteableRequired).Name;
            }
        }
    }
}