using System;
using Isop.CommandLine.Parse;
using Isop.Domain;

namespace Isop.CommandLine
{
    internal static class PropertyExtensions
    {
        [Obsolete("Temporary")]
        public static Argument ToArgument(this Property p, IFormatProvider formatProvider)
        {
            var argumentParam= ArgumentParameter.Parse(p.Name, formatProvider);
            return new Argument(
                required: p.Required,
                description: p.Description,
                parameter: argumentParam
            );
        }
    }
}