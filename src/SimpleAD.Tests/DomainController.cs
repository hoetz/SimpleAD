using System.DirectoryServices.ActiveDirectory;

namespace SimpleAD.Tests
{
    public class DomainController
    {
        public static DomainController NONE = new DomainController("NONE");


        private string _Value;
        public string Value
        {
            get { return this._Value; }
        }

        public DomainController(string fqdn)
        {
            this._Value = fqdn;
        }

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