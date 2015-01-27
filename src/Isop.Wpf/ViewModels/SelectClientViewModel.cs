using Isop.Client;
using Isop.Client.Json;
using Isop.Gui.Adapters;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Isop.Gui.ViewModels
{
    public class SelectClientViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private string _url;
        public string Url
        {
            get { return _url; }
            set
            {
                _url = value;
                PropertyChanged.SendPropertyChanged(this, "Url");
            }
        }

        private string _assembly;
        public string Assembly
        {
            get { return _assembly; }
            set
            {
                _assembly = value;
                PropertyChanged.SendPropertyChanged(this, "Assembly");
            }
        }

        public event Action Loaded;
        public virtual void OnLoad() 
        {
            if (Loaded != null)
            {
                Loaded.Invoke();
            }
        }

        internal IClient GetClient()
        {
            if (!String.IsNullOrEmpty(Assembly)) 
            {
                var build = new Build();
                build.ConfigurationFrom(System.Reflection.Assembly.LoadFile(Assembly));
                return new BuildClient(build);
            }
            if (!String.IsNullOrEmpty(Url)) 
            {
                return new JsonClient(new IsopClient(new JsonHttpClient(), Url));
            }
            return null;
        }
    }
}
