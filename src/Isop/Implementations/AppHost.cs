using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Isop.Abstractions;
using Isop.CommandLine;
using Isop.CommandLine.Lex;
using Isop.CommandLine.Parse;
using Isop.Domain;
using Isop.Help;
using Isop.Localization;

namespace Isop.Implementations;

internal sealed class AppHost : IAppHost
{
    internal readonly Abstractions.ToStrings ToStrings;
    internal readonly Recognizes Recognizes;
    internal readonly IServiceProvider ServiceProvider;
    internal readonly ControllerRecognizer ControllerRecognizer;
    internal readonly IOptions<Conventions> Conventions;
    internal readonly IOptions<AppHostConfiguration> Configuration;
    internal readonly TypeConverter TypeConverter;
    private HelpController? _helpController;
    private readonly IOptions<Texts> _texts;


    internal HelpController HelpController =>
        _helpController ??= ServiceProvider.GetService<HelpController>()
                        ?? new HelpController(
                                                _texts,
                                                Recognizes,
                                                Configuration,
                                                ServiceProvider,
                                Conventions);

    /// <summary>
    /// 
    /// </summary>
    public AppHost(IOptions<AppHostConfiguration>? options,
        IServiceProvider serviceProvider,
        Recognizes recognizes,
        TypeConverter typeConverter,
        Abstractions.ToStrings toStrings,
        IOptions<Texts>? texts,
        IOptions<Conventions>? conventions)
    {
        ToStrings = toStrings;
        Configuration = options ?? Options.Create(new AppHostConfiguration());
        Recognizes = recognizes;
        ServiceProvider = serviceProvider;
        Conventions = conventions ?? Options.Create(new Conventions());
        TypeConverter = typeConverter;
        ControllerRecognizer = new ControllerRecognizer(options,
            typeConverter, Conventions, Recognizes);
        _texts = texts ?? Options.Create(new Texts());
    }


    /// <summary>
    /// Parse command line arguments and return parsed arguments entity
    /// </summary>
    public IParsed Parse(IEnumerable<string> arg) => Parse(arg.ToList());

    private ParsedExpression Parse(IReadOnlyCollection<string> arg)
    {
        var argumentParser = new ArgumentParser(
            Recognizes.Properties,
            AllowInferParameter);
        var lexed = ArgumentLexer.Lex(arg).ToList();
        var parsedGlobalArgs = argumentParser.Parse(lexed, arg);
        if (!ControllerRecognizer.TryRecognize(arg, out var controllerAndMethodAndTokens))
            return new ParsedExpression(parsedGlobalArgs, this);

        var (controller, method) = controllerAndMethodAndTokens;
        return new ParsedExpression(parsedGlobalArgs.Merge(
            ControllerRecognizer.Parse(controller, method, arg)), this);
    }

    internal bool AllowInferParameter => !(Configuration.Value?.DisableAllowInferParameter ?? false);
    internal CultureInfo? CultureInfo => Configuration.Value?.CultureInfo;

    [Obsolete("Prefer HelpAsync")]
    public string Help()
    {
        using var output = new StringWriter(CultureInfo);
        Parse([Conventions.Value.Help]).Invoke(output);
        return output.ToString();
    }

    public async Task<string> HelpAsync()
    {
        using var output = new StringWriter(CultureInfo);
        await Parse([Conventions.Value.Help]).InvokeAsync(output);
        return output.ToString();
    }

    public IController Controller(string controllerName)
    {
        if (ControllerRecognizer.TryFindController(controllerName, out var controller))
        {
            return new ControllerExpression(controllerName, this, controller!);
        }
        throw new ControllerNotFoundException($"Unknown controller {controllerName}");
    }

    public IReadOnlyList<IController> Controllers =>
        Recognizes.Controllers.Select(c => Controller(c.GetName(Conventions.Value))).ToArray();
}
