using System;

namespace Isop.Gui
{
    public class Param
    {
        public Type Type { get; set; }
        public string Name { get; set; }
        private ArgumentWithOptions _ArgWithOptions { get; set; }
        public string Value { get; set; }
        public Param(Type type, string name, ArgumentWithOptions argWithOptions)
        {
            Type = type;
            Name = name;
            _ArgWithOptions = argWithOptions;
        }

        public ArgumentWithOptions ArgumentWithOptions()
        {
            if (null != _ArgWithOptions) return _ArgWithOptions;
            return new ArgumentWithOptions(Name);
        }

        public RecognizedArgument RecognizedArgument()
        {
            return new RecognizedArgument(ArgumentWithOptions(), Name, Value);
        }
    }
}