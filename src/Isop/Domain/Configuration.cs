using System;
using System.Globalization;
using System.Collections.Generic;
using Isop.Infrastructure;

namespace Isop.Domain
{
    public class Configuration
    {
        public Configuration()
        {
            Recognizes = new List<Controller>();
            Properties = new List<Property>();
            Formatter = new ToStringFormatter().Format;
        }
        public CultureInfo CultureInfo { get; set; }
        public IList<Controller> Recognizes { get; private set; }
        public IList<Property> Properties { get; private set; }

        public Func<Type, object> Factory { get; set; }

        public TypeConverterFunc TypeConverter { get; set; }

        public bool RecognizesHelp { get; set; }
        public Formatter Formatter{ get; set;}
    }
}

