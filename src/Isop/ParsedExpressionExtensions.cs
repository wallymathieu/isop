using System;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Isop.Abstractions;

namespace Isop
{
    public static class ParsedExpressionExtensions
    {
        
        /// <summary>
        /// 
        /// </summary>
        [Obsolete("Prefer InvokeAsync")]
        public static void Invoke(this IParsed parsed, TextWriter? output)
        {
            if (parsed is null) throw new ArgumentNullException(nameof(parsed));
            parsed.InvokeAsync(output).GetAwaiter().GetResult();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parsed"></param>
        /// <param name="out"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public static async Task<int> TryInvokeAsync(this IParsed parsed, TextWriter? @out = null, TextWriter? @error = null)
        {
            if (parsed is null) throw new ArgumentNullException(nameof(parsed));
            @out ??= Console.Out;
            error ??= Console.Error;
            try
            {
                if (parsed.Unrecognized.Count != 0) //Warning:
                {
                    await error.WriteLineAsync($@"Unrecognized arguments: 
{string.Join(",", parsed.Unrecognized.Select(arg => arg.Value).ToArray())}");
                    return 400;
                }
                await parsed.InvokeAsync(@out);
            }
            catch (TypeConversionFailedException ex)
            {
                await error.WriteLineAsync(
                    $"Could not convert argument {ex.Argument} with value {ex.Value} to type {ex.TargetType}");
                if (null != ex.InnerException)
                {
                    await error.WriteLineAsync("Inner exception: ");
                    await error.WriteLineAsync(ex.InnerException.Message);
                }

                return 400;
            }
            catch (MissingArgumentException ex)
            {
                await error.WriteLineAsync($"Missing argument(s): {string.Join(", ", ex.Arguments??Array.Empty<string>()).ToArray()}");

                await error.WriteLineAsync(await parsed.HelpAsync());
                return 400;
            }

            return 0;
        }
    }
}