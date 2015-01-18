using System;
using System.IO;
using System.Globalization;
using System.ComponentModel;

namespace Isop.Domain
{
    public class DefaultConverter
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