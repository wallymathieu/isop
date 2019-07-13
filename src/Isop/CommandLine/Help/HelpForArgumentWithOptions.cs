using System;
using System.Collections.Generic;
using System.Linq;

namespace Isop.CommandLine.Help
{
    using Isop.Help;
    using Parse;
    internal class HelpForArgumentWithOptions
    {
        private readonly HelpTexts helpTexts;
        private readonly IEnumerable<ArgumentWithOptions> _argumentWithOptionses;

        public HelpForArgumentWithOptions(HelpTexts helpTexts, IEnumerable<ArgumentWithOptions> argumentWithOptionses)
        {
            this.helpTexts = helpTexts;
            _argumentWithOptionses = argumentWithOptionses;
        }

        private static string Help(ArgumentWithOptions entity)
        {
            return string.Concat(entity.Argument.Help(), (string.IsNullOrEmpty(entity.Description)
                    ? ""
                    : "\t"+ entity.Description));
        }

        public string Help(string val = null)
        {
            if (string.IsNullOrEmpty(val))
                return helpTexts.TheArgumentsAre + Environment.NewLine +
                      string.Join(Environment.NewLine,
                                  _argumentWithOptionses.Select(ar => "  "+ Help(ar)).ToArray());
            return Help(_argumentWithOptionses.First(ar => ar.Argument.Prototype.Equals(val)));
        }

        public bool CanHelp(string val = null)
        {
            return string.IsNullOrEmpty(val)
                ? _argumentWithOptionses.Any()
                : _argumentWithOptionses.Any(ar => ar.Argument.Prototype.Equals(val));
        }
    }
}

