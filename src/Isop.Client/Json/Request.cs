using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Collections.Specialized;
using Newtonsoft.Json;

namespace Isop.Client.Json
{
    public class Request
    {
        public Uri Uri;
        public string Method;
        public string Data;
        public bool DoStream;

        public class Configure
        {
            private Request request;

            public Configure(Request request)
            {
                this.request = request;
            }

            public Configure Get()
            {
                request.Method = "GET";
                return this;
            }

            public Configure Post()
            {
                request.Method = "POST";
                return this;
            }

            public Configure Form(IDictionary<string, string> form)
            {
                request.Data = String.Join("&", form.Select(item => item.Key + "=" + WebUtility.UrlEncode(item.Value)));
                return this;
            }

            public Configure Stream()
            {
                request.DoStream = true;
                return this;
            }

            public Configure Json(Dictionary<string, string> form)
            {
                request.Data = JsonConvert.SerializeObject(form);
                return this;
            }
        }

        public Request(string url, Action<Configure> conf = null)
        {
            this.Uri = new Uri(url);
            if (conf != null)
            {
                conf(new Configure(this));
            }
        }

        public bool Post { get { return Method == "POST"; } }
        public bool Get { get { return Method == "GET"; } }
    }
}
