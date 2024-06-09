using System;
using System.Linq;
using Microsoft.Extensions.Options;
using Isop.Domain;

namespace Isop.CommandLine.Help;
internal sealed class HelpForArgumentWithOptions(IOptions<Localization.Texts> texts,
    Recognizes recognizes)
{
    private readonly Localization.Texts _texts = texts.Value ?? new Localization.Texts();

    private static string Help(ArgumentWithAction entity)
    {
        //var arg = entity.AsArgument();
        return string.Concat(entity.Help(), string.IsNullOrEmpty(entity.Description)
            ? ""
            : "\t" + entity.Description);
    }

    public string Help(string? val = null)
    {
        if (string.IsNullOrEmpty(val))
            return _texts.TheArgumentsAre + Environment.NewLine +
                  string.Join(Environment.NewLine,
                              recognizes.Properties.Select(ar => "  " + HelpForArgumentWithOptions.Help(ar)).ToArray());
        return HelpForArgumentWithOptions.Help(recognizes.Properties.First(ar => ar.Accept(val!)));
    }

    public bool CanHelp(string? val = null)
    {
        return string.IsNullOrEmpty(val)
            ? recognizes.Properties.Any()
            : recognizes.Properties.Any(ar => ar.Accept(val!));
    }
}

