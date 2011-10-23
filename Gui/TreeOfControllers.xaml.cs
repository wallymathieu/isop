using System.Collections.Generic;

namespace Gui
{
    public class TreeOfControllers
    {
        private IList<Controller> _controllers;
        public IList<Controller> Controllers
        {
            get { return _controllers; }
            set { _controllers = value; }
        }

        public TreeOfControllers()
        {
            Controllers = new List<Controller>() 
                              { 
                                  new Controller {Name = "1"},
                                  new Controller {Name = "2"},
                                  new Controller {Name = "3"}
                              };
        }
    }
}