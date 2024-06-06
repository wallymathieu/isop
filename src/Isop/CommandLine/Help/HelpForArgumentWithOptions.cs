using System;
using System.Linq;
using Microsoft.Extensions.Options;

namespace Isop.CommandLine.Help
{
    using Domain;

    internal class HelpForArgumentWithOptions(IOptions<Localization.Texts> texts,
        Recognizes recognizes,
        IOptions<Configuration> config)
    {
        private readonly Localization.Texts _texts = texts.Value ?? new Localization.Texts();

        private string Help(Property entity)
        {
            //var arg = entity.AsArgument();
            return string.Concat(entity.ToArgument(config?.Value?.CultureInfo).Help(), string.IsNullOrEmpty(entity.Description)
                ? ""
                : "\t"+ entity.Description);
        }

        public string Help(string? val = null)
        {
            if (string.IsNullOrEmpty(val))
                return _texts.TheArgumentsAre + Environment.NewLine +
                      string.Join(Environment.NewLine,
                                  recognizes.Properties.Select(ar => "  "+ Help(ar)).ToArray());
            return Help(recognizes.Properties.First(ar => ar.ToArgument(config?.Value?.CultureInfo).Accept(val)));
        }

        public bool CanHelp(string? val = null)
        {
            return string.IsNullOrEmpty(val)
                ? recognizes.Properties.Any()
                : recognizes.Properties.Any(ar => ar.ToArgument(config?.Value?.CultureInfo).Accept(val));
        }
    }
}

