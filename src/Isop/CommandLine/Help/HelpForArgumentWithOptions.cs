using System;
using System.Collections.Generic;
using System.Linq;

namespace Isop.CommandLine.Help
{
    using Parse;
    public class HelpForArgumentWithOptions
    {
        private readonly IEnumerable<ArgumentWithOptions> _argumentWithOptionses;
        public string TheArgumentsAre { get; set; }

        public HelpForArgumentWithOptions(IEnumerable<ArgumentWithOptions> argumentWithOptionses)
        {
            _argumentWithOptionses = argumentWithOptionses;
            TheArgumentsAre = "The arguments are:";
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
                return TheArgumentsAre + Environment.NewLine +
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

