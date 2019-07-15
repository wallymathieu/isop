using System;
using System.Linq;
using Microsoft.Extensions.Options;

namespace Isop.CommandLine.Help
{
    using Domain;

    internal class HelpForArgumentWithOptions
    {
        private readonly Localization.Texts _texts;
        private readonly Recognizes _recognizes;
        private readonly IOptions<Configuration> _config;

        public HelpForArgumentWithOptions(IOptions<Localization.Texts> texts, 
            Recognizes recognizes,
            IOptions<Configuration> config)
        {
            _texts = texts.Value ?? new Localization.Texts();
            _recognizes = recognizes;
            _config = config;
        }

        private string Help(Property entity)
        {
            //var arg = entity.AsArgument();
            return string.Concat(entity.ToArgument(_config?.Value?.CultureInfo).Help(), string.IsNullOrEmpty(entity.Description)
                ? ""
                : "\t"+ entity.Description);
        }

        public string Help(string val = null)
        {
            if (string.IsNullOrEmpty(val))
                return _texts.TheArgumentsAre + Environment.NewLine +
                      string.Join(Environment.NewLine,
                                  _recognizes.Properties.Select(ar => "  "+ Help(ar)).ToArray());
            return Help(_recognizes.Properties.First(ar => ar.ToArgument(_config?.Value?.CultureInfo).Accept(val)));
        }

        public bool CanHelp(string val = null)
        {
            return string.IsNullOrEmpty(val)
                ? _recognizes.Properties.Any()
                : _recognizes.Properties.Any(ar => ar.ToArgument(_config?.Value?.CultureInfo).Accept(val));
        }
    }
}

