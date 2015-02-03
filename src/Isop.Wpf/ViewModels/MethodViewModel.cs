using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using Isop.Gui;
using System.ComponentModel;
using Isop.Gui.ViewModels;
using Isop.Client.Transfer;
namespace Isop.Gui.ViewModels
{
    public class MethodViewModel : INotifyPropertyChanged, IReceiveResult
    {
        public MethodViewModel(Isop.Client.Transfer.Method m)
        {
            Method = m;
            Parameters = m.Parameters.Select(p => new ParamViewModel(p)).ToArray();
        }

        public Isop.Client.Transfer.Method Method { get; private set; }
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

        private IErrorMessage[] _Error;

        public IErrorMessage[] Errors
        {
            get { return _Error; }
            set
            {
                _Error = value;
                PropertyChanged.SendPropertyChanged(this, "Error");
                var parameters = Parameters.ToList();
                foreach (var param in Parameters)
                {
                    var error = (_Error ?? new IErrorMessage[0]).SingleOrDefault(p => p.Argument.Equals(param.Name));
                    param.Error = error != null ? error.Message : null;
                }
                Result = String.Join(", ", (_Error ?? new IErrorMessage[0]).Select(error => error.Message));//TODO: Fix
            }
        }
    }
}