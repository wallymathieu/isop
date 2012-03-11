using System;
using System.ComponentModel;

namespace Isop.WpfControls.ViewModels
{
    public class Param : INotifyPropertyChanged
    {
        public Type Type { get; private set; }
        public string Name { get; private set; }
        public bool Required { get { return ArgumentWithOptions.Required; } }
        private string _value;
        public string Value 
        { 
            get { return this._value; } 
            set 
            { 
                this._value = value; 
                if (PropertyChanged!=null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Value"));
                }
            }
        }
        public Param(Type type, string name, ArgumentWithOptions argWithOptions)
        {
            Type = type;
            Name = name;
            ArgumentWithOptions = argWithOptions;
        }

        public ArgumentWithOptions ArgumentWithOptions
        {
            get;
            private set;
        }

        public RecognizedArgument RecognizedArgument()
        {
            return new RecognizedArgument(ArgumentWithOptions, Name, Value);
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}