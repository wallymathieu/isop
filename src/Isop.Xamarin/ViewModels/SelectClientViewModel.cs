using Isop.Client;
using Isop.Client.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Isop.Xamarin
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
            if (JsonClient.CanLoad(Url)) 
            {
                return new JsonClient(new IsopClient(new JsonHttpClient(), Url));
            }
            return null;
        }
    }
}
