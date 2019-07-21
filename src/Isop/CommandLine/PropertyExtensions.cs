using System;

namespace Isop.CommandLine
{
    using Parse;
    using Domain;
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