using System;
using System.IO;
namespace Isop.Client.Json
{
    public class JsonResponse : IDisposable
    {
        public System.Net.HttpStatusCode HttpStatusCode { get; private set; }
        public string Data { get; private set; }
        public Stream Stream { get; private set; }

        public JsonResponse(System.Net.HttpStatusCode httpStatusCode, string response)
        {
            this.HttpStatusCode = httpStatusCode;
            this.Data = response;
        }

        public JsonResponse(System.Net.HttpStatusCode httpStatusCode, System.IO.Stream stream)
        {
            this.HttpStatusCode = httpStatusCode;
            this.Stream = stream;
            this.Data = null;
        }

        public void Dispose()
        {
            if (Stream != null) { Stream.Dispose(); Stream = null; }
        }
    }
}
