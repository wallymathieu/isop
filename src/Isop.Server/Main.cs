using Nancy.Hosting.Self;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Isop.Server
{
    class MainCls
    {
        public static void Main()
        {
            var conf = new HostConfiguration()
            {
                AllowChunkedEncoding = true,
                UrlReservations = { CreateAutomatically = true }
            };
            using (var host = new NancyHost(conf, new Uri("http://localhost:8080")))
            {
                Console.WriteLine("Started");
                host.Start();
                Console.ReadLine();
            }
        }
    }
}
