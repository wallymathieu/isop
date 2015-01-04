using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Isop.Gui.Http
{
    internal class HttpClient
    {
        protected internal static RequestError GetRequestException(WebException ex)
        {
            if (ex.Response != null)
            {
                using (var rstream = ex.Response.GetResponseStream())
                using (var reader = new StreamReader(rstream, Encoding.UTF8))
                {
                    var c = reader.ReadToEnd().Replace("\\n", "\n").Replace("\\r", "\r");
                    var resp = ((HttpWebResponse)ex.Response);
                    return new RequestError(resp.StatusCode, c, resp.Headers["ErrorType"]);
                }
            }
            return new RequestError(HttpStatusCode.InternalServerError, ex.Message, "InternalServerError");
        }
    }
}
