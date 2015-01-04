using System;
using System.IO;
namespace Isop.Gui
{
    public class JsonResponse : IDisposable
    {
        public RequestError RequestException { get; private set; }
        public string ErrorMessage { get { return RequestException != null ? RequestException.Message : null; } }
        public System.Net.HttpStatusCode HttpStatusCode { get; private set; }
        public string Data { get; private set; }
        public Stream Stream { get; private set; }

        public JsonResponse(System.Net.HttpStatusCode httpStatusCode, string response)
        {
            this.HttpStatusCode = httpStatusCode;
            this.Data = response;
        }

        public JsonResponse(RequestError requestException)
        {
            this.Data = null;
            this.RequestException = requestException;
            this.HttpStatusCode = requestException.HttpStatusCode;
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
