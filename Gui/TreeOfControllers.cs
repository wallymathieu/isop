using System.Collections.Generic;

namespace Isop.Gui
{
    public class TreeOfControllers
    {
        public IList<Controller> Controllers { get; set; }

        public TreeOfControllers()
        {
            Controllers = new List<Controller>
                              { 
                                  new Controller {Name = "1", Methods=new[]{
                                      new Method("a"){Parameters=new[]{new Param(typeof(string),"a"),new Param(typeof(string),"b"),new Param(typeof(string),"c"),new Param(typeof(string),"c"),new Param(typeof(string),"d"),new Param(typeof(string),"e")}},
                                      new Method("b"){Parameters=new[]{new Param(typeof(string),"a")}}
                                  }},
                                  new Controller {Name = "2", Methods=new[]{
                                      new Method("a"){Parameters=new[]{new Param(typeof(string),"a")}},
                                      new Method("b"){Parameters=new[]{new Param(typeof(string),"a")}}
                                  }},
                                  new Controller {Name = "3", Methods=new[]{
                                      new Method("a"){Parameters=new[]{new Param(typeof(string),"a")}},
                                      new Method("b"){Parameters=new[]{new Param(typeof(string),"a")}}
                                  }}
                              };
        }
    }
}