using System;

namespace Isop.Gui
{
    public class Param
    {
        public Type Type { get; set; }
        public string Name { get; set; }
        public bool Required { get { return _argWithOptions.Required; } }
        private readonly ArgumentWithOptions _argWithOptions;
        public string Value { get; set; }
        public Param(Type type, string name, ArgumentWithOptions argWithOptions)
        {
            Type = type;
            Name = name;
            _argWithOptions = argWithOptions;
        }

        public ArgumentWithOptions ArgumentWithOptions
        {
            get
            {
                return _argWithOptions;
            }
        }

        public RecognizedArgument RecognizedArgument()
        {
            return new RecognizedArgument(ArgumentWithOptions, Name, Value);
        }
    }
}