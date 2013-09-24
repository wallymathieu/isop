using System.Collections.Generic;
using Isop.Controller;

namespace Isop.WpfControls.ViewModels
{
    public class Method
    {
        private readonly HelpController _helpController;

        public Method(string name, string className, HelpController helpController)
        {
            _helpController = helpController;
            Name = name;
            ClassName = className;
        }

        public string Name { get; set; }
        public string ClassName { get; set; }

        public IEnumerable<Param> Parameters { get; set; }
        public string Help { get { return _helpController.Index(ClassName, Name); } }

    }
}