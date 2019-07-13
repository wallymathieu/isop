using System;
using System.Collections.Generic;
using System.Linq;
using Isop.CommandLine.Parse;

namespace Isop.Domain
{
    internal class GlobalArguments
    {
        private RecognizesConfiguration recognizesConfiguration;

        public GlobalArguments(RecognizesConfiguration recognizesConfiguration)
        {
            this.recognizesConfiguration = recognizesConfiguration;
        }

        public IEnumerable<ArgumentWithOptions> GlobalParameters
        {
            get
            {
                return recognizesConfiguration.Properties.Select(p =>
                    new ArgumentWithOptions(
                        type: p.Type,
                        argument: p.Name,
                        action: p.Action,
                        required: p.Required,
                        description: p.Description
                        )).ToList();
            }
        }
    }
}

