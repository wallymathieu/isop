using System.Globalization;

namespace Isop;
/// <summary>
/// Main configuration of a command line application.
/// </summary>
public class AppHostConfiguration
{
    /// <summary>
    /// The culture info for the console application
    /// </summary>
    public CultureInfo? CultureInfo { get; set; }

    /// <summary>
    /// If you want to disable infer parameter
    /// </summary>
    public bool DisableAllowInferParameter { get; set; }
}
