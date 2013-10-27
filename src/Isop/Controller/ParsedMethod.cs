using System;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Collections;
using Isop.Parse;

namespace Isop.Controller
{
    public class ParsedMethod : ParsedArguments
    {
        public ParsedMethod(ParsedArguments parsedArguments)
            : base(parsedArguments)
        {
        }
        public Func<Type,Object> Factory { get; set; }

        public Type RecognizedClass { get; set; }
        public MethodInfo RecognizedAction { get; set; }

        public IEnumerable<object> RecognizedActionParameters { get; set; }
     
        public override void Invoke(TextWriter cout)
        {
         var instance = Factory(RecognizedClass);
         
         var retval = RecognizedAction.Invoke(instance, RecognizedActionParameters.ToArray());
            if (retval == null) return;
            if (retval is string)
            {
                cout.Write(retval as string);
            }else if (retval is IEnumerable)
            {
                foreach (var item in (retval as IEnumerable)) {
                    cout.Write(item.ToString());
                }
            }else
            {
                cout.Write(retval.ToString());
            }
        }
    }
}

