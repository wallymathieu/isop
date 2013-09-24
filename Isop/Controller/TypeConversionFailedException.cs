using System;

namespace Isop.Controller
{
   
    [Serializable]
    public class TypeConversionFailedException : Exception
    {
        public string Argument;
        public string Value;
        public Type TargetType;
     public TypeConversionFailedException ()
     {
     }
     
     public TypeConversionFailedException (string message) : base (message)
     {
     }
     
     public TypeConversionFailedException (string message, Exception inner) : base (message, inner)
     {
     }
     
    }
}

