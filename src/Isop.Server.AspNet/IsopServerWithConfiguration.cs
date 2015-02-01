using Example;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Isop.Server.AspNet
{
    class IsopServerWithConfiguration:IsopServerFromBuild
    {
        public IsopServerWithConfiguration()
            :base(GetBuild)
        {
        }

        private static Build GetBuild() 
        {
            var parserBuilder = new Build()
                      .ShouldRecognizeHelp()
                      .Recognize(typeof(MyController))
                      .Recognize(typeof(CustomerController));
            return parserBuilder;
        }
    }
}
