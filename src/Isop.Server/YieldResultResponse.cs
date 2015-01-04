using Nancy;
using System.Collections.Generic;
using System.Text;

namespace Isop.Server
{
    class YieldResultResponse:Response
    {
        private static Encoding utf8 = Encoding.UTF8;
        private IEnumerable<string> Result;

        public YieldResultResponse(IEnumerable<string> result)
        {
            this.Result = result;
            this.Headers.Add("Transfer-Encoding", "Chunked");
            ContentType = "text/plain";
            Contents = s =>
            {
                foreach (var item in Result)
                {
                    byte[] bytes = utf8.GetBytes(item);
                    s.Write(bytes, 0, bytes.Length);
                    s.Flush();
                }
            };
        }
    }
}
