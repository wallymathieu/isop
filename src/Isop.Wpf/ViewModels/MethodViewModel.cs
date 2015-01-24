using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using Isop.Gui;
using System.ComponentModel;
using Isop.Gui.ViewModels;
using Isop.Client.Models;
namespace Isop.Gui.ViewModels
{
    public class MethodViewModel : INotifyPropertyChanged, IReceiveResult
    {
        public MethodViewModel(Isop.Client.Models.Method m)
        {
            Method = m;
            Parameters = m.Parameters.Select(p => new ParamViewModel(p)).ToArray();
        }

        public Isop.Client.Models.Method Method { get; private set; }
        public string Name { get { return Method.Name; } }
        public string ClassName { get { return Method.ClassName; } }

        public IEnumerable<ParamViewModel> Parameters { get; private set; }
        public string Help { get { return Method.Help; } }

        public string Url { get { return Method.Url; } }

        public event PropertyChangedEventHandler PropertyChanged;

        private string _result;

        public string Result
        {
            get { return _result; }
            set
            {
                _result = value;
                PropertyChanged.SendPropertyChanged(this, "Result");
            }
        }

        private string _ErrorMessage;

        public string ErrorMessage
        {
            get { return _ErrorMessage; }
            set
            {
                _ErrorMessage = value;
                PropertyChanged.SendPropertyChanged(this, "ErrorMessage");
                // TODO: better gui
                Result = _ErrorMessage;
            }
        }
        private IErrorMessage _Error;

        public IErrorMessage Error
        {
            get { return _Error; }
            set
            {
                _Error = value;
                PropertyChanged.SendPropertyChanged(this, "Error");
            }
        }
    }
}