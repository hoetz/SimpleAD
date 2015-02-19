using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Dynamic;
using System.Linq;

namespace SimpleAD
{
    internal static class DirectoryEntryExtensions
    {
        public static dynamic ToDynamicPropertyCollection(this DirectoryEntry e, IEnumerable<string> propertiesToLoad)
        {
            var effectivePropList=e.Properties.Cast<PropertyValueCollection>();

            ExpandoObject dyn = new ExpandoObject();
            if (propertiesToLoad == null)
            {
                foreach (PropertyValueCollection pro in e.Properties.Cast<PropertyValueCollection>())
                {
                    ((IDictionary<string, object>)dyn)[pro.PropertyName] = pro.PrettyPropertyValue();
                }
            }
            else
            {
                foreach (var prop in propertiesToLoad)
                {
                    ((IDictionary<string, object>)dyn)[prop] = e.Properties[prop].Value;
                }
            }
            ((IDictionary<string, object>)dyn)["NativeGuid"] = e.NativeGuid;
            ((IDictionary<string, object>)dyn)["Guid"] = e.Guid;
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

                case "pwdLastSet":
                    return LargeIntegerToDateTime(valCol);

                default: return valCol.Value;
            }
        }

        private static object LargeIntegerToDateTime(PropertyValueCollection valCol)
        {
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