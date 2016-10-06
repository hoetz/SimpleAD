using ActiveDs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices;
using System.Dynamic;
using System.Linq;
using Westwind.Utilities;

namespace SimpleAD
{
    internal static class DirectoryEntryExtensions
    {
        public static dynamic ToDynamicPropertyCollection(this DirectoryEntry e, IEnumerable<string> propertiesToLoad)
        {
            var effectivePropList=e.Properties.Cast<PropertyValueCollection>();

            DynamicActiveDirectoryObject dyn = new DynamicActiveDirectoryObject(e);
            if (propertiesToLoad == null)
            {
                foreach (PropertyValueCollection pro in e.Properties.Cast<PropertyValueCollection>())
                {
                    dyn[pro.PropertyName] = pro.PrettyPropertyValue();
                }
            }
            else
            {
                foreach (var prop in propertiesToLoad)
                {
                    try
                    {
                        dyn[prop] = e.Properties[prop].PrettyPropertyValue();
                    }
                    catch
                    {
                        Debug.WriteLine($"WARNING: Could not bind property {prop} from {e.Path}");
                    }
                }
            }
            dyn["NativeGuid"] = e.NativeGuid;
            dyn["Guid"] = e.Guid;
            return dyn;
        }

        private static object PrettyPropertyValue(this PropertyValueCollection valCol)
        {
            switch (valCol.PropertyName)
            {
                case "accountExpires":
                    return LargeIntegerToDateTime(valCol);

                case "badPasswordTime":
                    return LargeIntegerToDateTime(valCol);

                case "lastLogoff":
                    return LargeIntegerToDateTime(valCol);

                case "lastLogon":
                    return LargeIntegerToDateTime(valCol);

                case "lastLogonTimestamp":
                    return LargeIntegerToDateTime(valCol);

                case "pwdLastSet":
                    return LargeIntegerToDateTime(valCol);

                case "lockoutTime":
                    return LargeIntegerToDateTime(valCol);

                case"uSNChanged":
                    return LargeIntegerToDateTime(valCol);

                case "uSNCreated":
                    return LargeIntegerToDateTime(valCol);

                case "uSNDSALastObjRemoved":
                    return LargeIntegerToDateTime(valCol);

                case "uSNLastObjRem":
                    return LargeIntegerToDateTime(valCol);

                case "uSNSource":
                    return LargeIntegerToDateTime(valCol);

                case "msExchRecipientTypeDetails":
                    return LargeInteger(valCol);

                case "msExchVersion":
                    return LargeInteger(valCol);

                case "msExchMailboxSecurityDescriptor":
                    return ToSecurityDescriptor(valCol);

                default: return valCol.Value;
            }
        }

        private static object ToSecurityDescriptor(PropertyValueCollection valCol)
        {
            SecurityDescriptor sd = (SecurityDescriptor)valCol.Value;
            AccessControlList acl = (AccessControlList)sd.DiscretionaryAcl;
            String m_Trustee = "";
            String m_AccessMask = "";
            String m_AceType = "";
            String m_ReturnValue = "";

            foreach (AccessControlEntry ace in (IEnumerable)acl)
            {
                m_Trustee = m_Trustee + "," + ace.Trustee;
                m_AccessMask = m_AccessMask + "," + ace.AccessMask.ToString();
                m_AceType = m_AceType + "," + ace.AceType.ToString();

            }
            m_ReturnValue = "Trustee: " + m_Trustee + " " + "AccessMask: " + m_AccessMask + "AceType: " + m_AceType;
            return m_ReturnValue;

        }

        private static object LargeInteger(PropertyValueCollection valCol)
        {
            if (valCol.Value == null)
                return 0L;
            var asLong = Converter.FromLargeIntegerToLong(valCol.Value);
            return asLong.ToString();
        }

        private static object LargeIntegerToDateTime(PropertyValueCollection valCol)
        {
            if (valCol.Value == null)
                return 0L;
            var asLong = Converter.FromLargeIntegerToLong(valCol.Value);
            if (asLong == long.MaxValue || asLong <= 0 || DateTime.MaxValue.ToFileTime() <= asLong)
            {
                return DateTime.MaxValue;
            }
            else
            {
                return DateTime.FromFileTimeUtc(asLong);
            }
        }
    }
}