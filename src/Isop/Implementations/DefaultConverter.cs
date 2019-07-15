using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;

namespace Isop.Implementations
{
    internal class DefaultConverter
    {
        public object ConvertFrom(Type type, string s, CultureInfo cultureInfo)
        {
            if (type == typeof(FileStream))
            {
                return new FileStream(s, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            }
            return TypeDescriptor.GetConverter(type).ConvertFrom(null, cultureInfo, s);
        }
    }
}