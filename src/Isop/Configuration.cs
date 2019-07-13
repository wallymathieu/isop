using System.Globalization;

namespace Isop
{
    public class Configuration
    {
        public CultureInfo CultureInfo { get; set; }
        public bool RecognizeHelp { get; set; }
        public bool DisableAllowInferParameter { get; set; }
    }
}

