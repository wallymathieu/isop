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
            if (BuildClient.CanLoad(Url)) 
            {
                var build = new Build();
                build.ConfigurationFrom(System.Reflection.Assembly.LoadFile(Url));
                return new BuildClient(build);
            }
            if (JsonClient.CanLoad(Url)) 
            {
                return new JsonClient(new IsopClient(new JsonHttpClient(), Url));
            }
            return null;
        }
    }
}
