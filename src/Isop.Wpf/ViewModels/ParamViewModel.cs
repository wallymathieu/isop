using System;
using System.ComponentModel;

namespace Isop.Gui.ViewModels
{
    public class ParamViewModel : INotifyPropertyChanged
    {
        public ParamViewModel(Isop.Client.Transfer.Param param)
        {
            Parameter = param;
            Index = counter++;
            Type = GetType(Parameter);
        }

        private static Type GetType(Isop.Client.Transfer.Param p)
        {
            return Type.GetType(p.Type);
        }
        public Isop.Client.Transfer.Param Parameter { get; private set; }
        public Type Type { get; private set; }
        public string Name { get { return Parameter.Name; } }
        public int Index { get; private set; }
        public bool Required { get { return Parameter.Required; } }
        public string Value
        {
            get { return Parameter.Value; }
            set
            {
                Parameter.Value = value;
                PropertyChanged.SendPropertyChanged(this, "Value");
            }
        }
        public string Error
        {
            get { return _error; }
            set
            {
                _error = value;
                PropertyChanged.SendPropertyChanged(this, "Error");
            }
        }

        private static int counter = 0;

        public event PropertyChangedEventHandler PropertyChanged;
        private string _error;

    }
}