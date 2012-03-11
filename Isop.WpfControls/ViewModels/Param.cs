using System;
using System.ComponentModel;

namespace Isop.WpfControls.ViewModels
{
    public class Param : INotifyPropertyChanged
    {
        public Type Type { get; set; }
        public string Name { get; set; }
        public bool Required { get { return _argWithOptions.Required; } }
        private readonly ArgumentWithOptions _argWithOptions;
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

        public event PropertyChangedEventHandler PropertyChanged;
    }
}