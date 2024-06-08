using System;

namespace Isop.CommandLine
{
    using Parse;
    using Domain;
    internal static class PropertyExtensions
    {
        public static Argument ToArgument(this Property p)
        {
            return new Argument(
                required: p.Required,
                description: p.Description,
                parameter: ArgumentParameter.Empty(p.Name)
            );
        }
    }
}