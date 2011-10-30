﻿using System.Collections.ObjectModel;

namespace Isop.WpfControls.ViewModels
{
    public class Method
    {
        public Method(string name, string className)
        {
            Name = name;
            ClassName = className;
        }

        public string Name { get; set; }
        public string ClassName { get; set; }

        public ObservableCollection<Param> Parameters { get; set; }

        public ParsedArguments GetParsedArguments()
        {
            return Parameters.GetParsedArguments();
        }
    }
}