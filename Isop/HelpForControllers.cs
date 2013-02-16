using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Isop
{
    public class HelpForControllers
    {
        readonly IEnumerable<ControllerRecognizer> _classAndMethodRecognizers;
        private readonly TypeContainer _container;
        private readonly MethodInfoFinder _methodInfoFinder = new MethodInfoFinder();
        private readonly HelpXmlDocumentation _helpXmlDocumentation;
        public string TheCommandsAre { get; set; }
		public string TheSubCommandsFor { get; set; }
        public string HelpCommandForMoreInformation { get; set; }

        public string HelpSubCommandForMoreInformation { get; set; }

        public HelpForControllers(IEnumerable<ControllerRecognizer> classAndMethodRecognizers, TypeContainer container,  HelpXmlDocumentation helpXmlDocumentation)
        {
            _container=container;
            _classAndMethodRecognizers = classAndMethodRecognizers;
            this._helpXmlDocumentation = helpXmlDocumentation;
            HelpSubCommandForMoreInformation = "Se 'COMMANDNAME' help <command> <subcommand> for more information";

            HelpCommandForMoreInformation = "Se 'COMMANDNAME' help <command> for more information";
            TheCommandsAre = "The commands are:";
			TheSubCommandsFor = "The sub commands for ";
        }
        private string Description(Type t, MethodInfo method=null)
        { 
            var description = _methodInfoFinder.Match(t.GetMethods(),
                returnType:typeof(string),
                name:"help",
                parameters:new []{typeof(string)});
            var descr = string.Empty;
            if (null==description) {
                if (null==method){
                    descr= _helpXmlDocumentation.GetDescriptionForType(t);
                }else{
                    descr= _helpXmlDocumentation.GetDescriptionForMethod(method);
                }
            }else{
                var obj = _container.CreateInstance(t);
            
                descr =(string) description.Invoke(obj,new[]{(method!=null? method.Name:null)});
            }
            if (string.IsNullOrEmpty(descr))
                return string.Empty;
            return "  "+descr;
        }
        
        private string HelpFor(ControllerRecognizer cmr,bool simpleDescription)
        {
            if (simpleDescription) return cmr.ClassName()+ Description(cmr.Type);
            
            return cmr.ClassName()
                +Environment.NewLine
                +Environment.NewLine
                +String.Join(Environment.NewLine, cmr.GetMethods()
                        .Select(m=>"  "+m.Name+Description(cmr.Type,m)).ToArray());
        }

        private string HelpForAction(ControllerRecognizer cmr, string action)
        {
            var methodhelp = cmr.GetMethodHelp(action);
            var parameters = methodhelp.MethodParameters().Select(r => DescriptionAndHelp(cmr.Type, methodhelp, r));
            if (parameters.Any())
                return string.Format(@"{0} {1}
And accept the following parameters:
{2}", methodhelp.Name, Description(cmr.Type, methodhelp.Method), String.Join(", ",
    parameters));
            else
                return string.Format(@"{0} {1}", methodhelp.Name, Description(cmr.Type, methodhelp.Method));
        }

        private string DescriptionAndHelp(Type t, ControllerRecognizer.MethodHelp m, ArgumentWithOptions r)
        {
            return r.Help();
        }
        //Description(cmr.Type, methodInfo,
        public string Help(string val = null, string action=null)
        {
            if (string.IsNullOrEmpty(val)) return TheCommandsAre + Environment.NewLine +
                   String.Join(Environment.NewLine,
                               _classAndMethodRecognizers
                               .Where(cmr=>cmr.Type!=typeof(HelpController))
                               .Select(cmr => "  " + HelpFor(cmr,true)).ToArray())
                   + Environment.NewLine
                   + Environment.NewLine
                   + HelpCommandForMoreInformation;

			var controllerRecognizer = _classAndMethodRecognizers.First(cmr => 
                cmr.ClassName().Equals(val, StringComparison.OrdinalIgnoreCase));
            if (string.IsNullOrEmpty(action))
            {
                return TheSubCommandsFor+
                       HelpFor(controllerRecognizer,false)
                       + Environment.NewLine
                       + Environment.NewLine
                       + HelpSubCommandForMoreInformation;
            }
            return HelpForAction(controllerRecognizer, action);
        }

        public bool CanHelp(string val = null, string action=null)
        {
            return string.IsNullOrEmpty(val) 
                ? _classAndMethodRecognizers.Any(cmr => cmr.Type != typeof(HelpController)) 
                : _classAndMethodRecognizers.Any(cmr=>cmr.ClassName().Equals(val,StringComparison.OrdinalIgnoreCase));
        }
    }
}
