using System.DirectoryServices;
using Westwind.Utilities;

namespace SimpleAD
{
    public class DynamicActiveDirectoryObject : Expando
    {
        private readonly DirectoryEntry sourceEntry;

        public DynamicActiveDirectoryObject(DirectoryEntry sourceEntry)
        {
            this.sourceEntry = sourceEntry;
        }

        public void Save()
        {
            foreach (var item in this.GetDynamicMemberNames())
            {
                if (item != "NativeGuid" && item != "Guid")
                    this.sourceEntry.Properties[item].Value = this[item];
            }
            this.sourceEntry.CommitChanges();
        }
    }
}