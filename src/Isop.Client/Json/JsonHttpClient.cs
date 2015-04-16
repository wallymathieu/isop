using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Isop.Client.Json
{
    public class JsonHttpClient : IJSonHttpClient
    {
        public
        #if !PCL
        async 
        #endif
        Task<JsonResponse> Request(Request jsonRequest)
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(jsonRequest.Uri);
                {
                    var r = request;
                    r.Method = jsonRequest.Method;
                    //r.ContentType = "application/json; charset=utf-8";
                    r.Accept = "application/json";
                    if (!string.IsNullOrEmpty(jsonRequest.Data))
                    {
                        request.ContentType = "application/x-www-form-urlencoded";
                        var bytes = Encoding.UTF8.GetBytes(jsonRequest.Data);
                        #if !PCL
                        using (var stream = await r.GetRequestStreamAsync())
                        {
                            stream.Write(bytes, 0, bytes.Length);
                        }
                        #else
                        r.BeginGetRequestStream(ar=>{
                            var req = (HttpWebRequest)ar.AsyncState;
                            using (var stream = req.EndGetRequestStream(ar))
                            {
                                stream.Write(bytes, 0, bytes.Length);
                            }
                        }, request);
                        #endif
                    }
                };
                #if !PCL
                var response = await request.GetResponseAsync();
                if (jsonRequest.DoStream)
                {
                    return new JsonResponse(((HttpWebResponse)response).StatusCode, response.GetResponseStream());
                }
                using (var rstream = response.GetResponseStream())
                using (var reader = new StreamReader(rstream, Encoding.UTF8))
                {
                    var c = reader.ReadToEnd();
                    return new JsonResponse(((HttpWebResponse)response).StatusCode, c);
                }
                #else
                Task<WebResponse> task= Task.Factory.FromAsync<WebResponse>(
                    request.BeginGetResponse,
                    request.EndGetResponse,
                    request
                );

                return task.ContinueWith<JsonResponse>(response=>{
                    if (jsonRequest.DoStream)
                    {
                        return new JsonResponse(((HttpWebResponse)response.Result).StatusCode, response.Result.GetResponseStream());
                    }
                    using (var rstream = response.Result.GetResponseStream())
                    using (var reader = new StreamReader(rstream, Encoding.UTF8))
                    {
                        var c = reader.ReadToEnd();
                        return new JsonResponse(((HttpWebResponse)response.Result).StatusCode, c);
                    }
                });
                #endif
            }
            catch (WebException ex)
            {
                throw GetRequestException(ex);
            }
            catch (Exception ex)
            {
                if (ex.GetBaseException() is WebException)
                    throw GetRequestException((WebException)ex.GetBaseException());
                throw;
            }
        }

        protected internal static RequestException GetRequestException(WebException ex)
        {
            if (ex.Response != null)
            {
                using (var rstream = ex.Response.GetResponseStream())
                using (var reader = new StreamReader(rstream, Encoding.UTF8))
                {
                    var c = reader.ReadToEnd();
                    var resp = ((HttpWebResponse)ex.Response);
                    return new RequestException(resp.StatusCode, c);
                }
            }
            return new RequestException(HttpStatusCode.InternalServerError, ex.Message);
        }
    }
}
