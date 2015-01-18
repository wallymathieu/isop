using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Veil.Handlebars;
using Veil.Parser;

namespace Isop.Server
{
    public class CustomParserRegistration : ITemplateParserRegistration
    {
        public IEnumerable<string> Keys { get { return new[] { "html" }; } }

        public Func<ITemplateParser> ParserFactory
        {
            get { return () => new HandlebarsParser(); }
        }
    }
}
