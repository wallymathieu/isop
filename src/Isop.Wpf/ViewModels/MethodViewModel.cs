using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using Isop.Gui;
using System.ComponentModel;
using Isop.Gui.ViewModels;

namespace Isop.Gui.ViewModels
{
    public class MethodViewModel : INotifyPropertyChanged, IReceiveResult
    {
        private Isop.Gui.Models.Method data;
        public MethodViewModel(Isop.Gui.Models.Method m, IIsopClient client)
        {
            data = m;
            Parameters = m.Parameters.Select(p => new ParamViewModel(p)).ToArray();
            isopClient = client;
        }

        public string Name { get { return data.Name; } }
        public string ClassName { get { return data.ClassName; } }

        public IEnumerable<ParamViewModel> Parameters { get; private set; }
        public string Help { get { return data.Help; } }
        private IIsopClient isopClient;

        public string Url { get { return data.Url; } }

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
        public async Task<IReceiveResult> Invoke()
        {
            return await isopClient.Invoke(this.data, this);
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
        private object _Error;

        public object Error
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