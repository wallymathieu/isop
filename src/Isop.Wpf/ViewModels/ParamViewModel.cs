using System;
using System.ComponentModel;

namespace Isop.Gui.ViewModels
{
    public class ParamViewModel : INotifyPropertyChanged
    {
        private Isop.Gui.Models.Param data;
        public ParamViewModel(Isop.Gui.Models.Param param)
        {
            data = param;
            Index = counter++;
            Type = GetType(data);
        }

        private static Type GetType(Isop.Gui.Models.Param p)
        {
            return Type.GetType(p.Type);
        }

        public Type Type { get; private set; }
        public string Name { get { return data.Name; } }
        public int Index { get; private set; }
        public bool Required { get { return data.Required; } }
        public string Value
        {
            get { return data.Value; }
            set
            {
                data.Value = value;
                PropertyChanged.SendPropertyChanged(this, "Value");
            }
        }

        private static int counter = 0;

        public event PropertyChangedEventHandler PropertyChanged;
    }
}