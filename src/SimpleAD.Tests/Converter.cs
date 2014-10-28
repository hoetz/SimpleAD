using System;
using System.Reflection;

namespace SimpleAD.Tests
{
    public static class Converter
    {
        public static long FromLargeIntegerToLong(object largeInteger)
        {
            Type type = largeInteger.GetType();

            int highPart = (int)type.InvokeMember("HighPart", BindingFlags.GetProperty, null, largeInteger, null);
            int lowPart = (int)type.InvokeMember("LowPart", BindingFlags.GetProperty | BindingFlags.Public, null, largeInteger, null);

            return (long)highPart << 32 | (uint)lowPart;
        }
    }
}