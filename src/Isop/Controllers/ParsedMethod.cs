using System;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Collections;
using Isop.Parse;

namespace Isop.Controllers
{
    public class ParsedMethod : ParsedArguments
    {
        public ParsedMethod(ParsedArguments parsedArguments)
            : base(parsedArguments)
        {
        }
        public Func<Type, Object> Factory { get; set; }

        public Type RecognizedClass { get; set; }
        public MethodInfo RecognizedAction { get; set; }

        public IEnumerable<object> RecognizedActionParameters { get; set; }

        public override IEnumerable<string> Invoke()
        {
            var instance = Factory(RecognizedClass);

            var retval = RecognizedAction.Invoke(instance, RecognizedActionParameters.ToArray());
            if (retval != null)
            {
                if (retval is string)
                {
                    yield return (retval as string);
                }
                else if (retval is IEnumerable)
                {
                    foreach (var item in (retval as IEnumerable))
                    {
                        yield return (item.ToString());
                    }
                }
                else
                {
                    yield return (retval.ToString());
                }
            }
        }
    }
}

