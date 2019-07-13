using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;

namespace Isop.CommandLine.Help
{
    using Isop.Domain;
    using Isop.Help;
    using Parse;
    internal class HelpForArgumentWithOptions
    {
        private readonly Localization.Texts helpTexts;
        private readonly GlobalArguments _argumentWithOptionses;

        public HelpForArgumentWithOptions(IOptions<Localization.Texts> helpTexts, GlobalArguments argumentWithOptionses)
        {
            this.helpTexts = helpTexts.Value ?? new Localization.Texts();
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
                                  _argumentWithOptionses.GlobalParameters.Select(ar => "  "+ Help(ar)).ToArray());
            return Help(_argumentWithOptionses.GlobalParameters.First(ar => ar.Argument.Prototype.Equals(val)));
        }

        public bool CanHelp(string val = null)
        {
            return string.IsNullOrEmpty(val)
                ? _argumentWithOptionses.GlobalParameters.Any()
                : _argumentWithOptionses.GlobalParameters.Any(ar => ar.Argument.Prototype.Equals(val));
        }
    }
}

