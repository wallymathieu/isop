using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
namespace Isop
{
    public class HelpController
    {
        readonly HelpForArgumentWithOptions _helpForArgumentWithOptions;
        readonly HelpForControllers _helpForClassAndMethod;

        public HelpController(
            HelpForArgumentWithOptions helpForArgumentWithOptions, 
            HelpForControllers helpForClassAndMethod)
        {
            _helpForArgumentWithOptions = helpForArgumentWithOptions;
            _helpForClassAndMethod = helpForClassAndMethod;
        }

        public string Index()
        {
            var sb = new StringBuilder();
            if (_helpForArgumentWithOptions.CanHelp())
            {
              sb.AppendLine(_helpForArgumentWithOptions.Help());
            }
            if (_helpForClassAndMethod.CanHelp())
            {
                sb.AppendLine(_helpForClassAndMethod.Help());
            }
            return sb.ToString().Trim(' ','\t','\r','\n');
        }
		public string Index(string command)
        {
			if (String.IsNullOrEmpty(command))
				return Index();
            var sb = new StringBuilder();
            if (_helpForArgumentWithOptions.CanHelp(command))
            {
              sb.AppendLine(_helpForArgumentWithOptions.Help(command));
            }
            if (_helpForClassAndMethod.CanHelp(command))
            {
                sb.AppendLine(_helpForClassAndMethod.Help(command));
            }
            return sb.ToString().Trim(' ','\t','\r','\n');
        }
    }

    public class HelpForArgumentWithOptions
    {
        readonly IEnumerable<ArgumentWithOptions> _argumentWithOptionses;
        public string TheArgumentsAre { get; set; }

        public HelpForArgumentWithOptions(IEnumerable<ArgumentWithOptions> argumentWithOptionses)
        {
            _argumentWithOptionses = argumentWithOptionses;
            TheArgumentsAre = "The arguments are:";
        }

        public string Help(string val=null)
        {
			if (string.IsNullOrEmpty(val))
	            return TheArgumentsAre + Environment.NewLine +
                   String.Join(Environment.NewLine,
                               _argumentWithOptionses.Select(ar => "  " + ar.Help()).ToArray());
        	return _argumentWithOptionses.First(ar=>ar.Argument.Prototype.Equals(val)).Help();
		}

        public bool CanHelp(string val=null)
        {
            return string.IsNullOrEmpty(val) 
                ? _argumentWithOptionses.Any() 
                : _argumentWithOptionses.Any(ar=>ar.Argument.Prototype.Equals(val));
        }
    }
    public class HelpForControllers
    {
        readonly IEnumerable<ControllerRecognizer> _classAndMethodRecognizers;
        private readonly TypeContainer _container;
        public string TheCommandsAre { get; set; }
		public string TheSubCommandsFor { get; set; }
        public string HelpCommandForMoreInformation { get; set; }

        public string HelpSubCommandForMoreInformation { get; set; }

        public HelpForControllers(IEnumerable<ControllerRecognizer> classAndMethodRecognizers, TypeContainer container)
        {
            _container=container;
            _classAndMethodRecognizers = classAndMethodRecognizers;

            HelpSubCommandForMoreInformation = "Se 'COMMANDNAME' help <command> <subcommand> for more information";

            HelpCommandForMoreInformation = "Se 'COMMANDNAME' help <command> for more information";
            TheCommandsAre = "The commands are:";
			TheSubCommandsFor = "The sub commands for ";
        }
        private string Description(Type t,MethodInfo method=null)
        { 
            var description = t.GetMethods().FirstOrDefault(m=>
                m.Name.Equals("help",StringComparison.OrdinalIgnoreCase));
            //TODO: should match parameters to make sure that it can accept 1 param of type string
            if (null==description) return string.Empty;
            
            var obj = _container.CreateInstance(t);
            
            return "  "+description.Invoke(obj,new[]{(method!=null? method.Name:null)});
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
        
        public string Help(string val=null)
        {
            if (string.IsNullOrEmpty(val)) return TheCommandsAre + Environment.NewLine +
                   String.Join(Environment.NewLine,
                               _classAndMethodRecognizers
                               .Where(cmr=>cmr.Type!=typeof(HelpController))
                               .Select(cmr => "  " + HelpFor(cmr,true)).ToArray())
                   + Environment.NewLine
                   + Environment.NewLine
                   + HelpCommandForMoreInformation;
			
			return TheSubCommandsFor+
				HelpFor(_classAndMethodRecognizers.First(cmr=>cmr.ClassName().Equals(val,StringComparison.OrdinalIgnoreCase)),false)
					+ Environment.NewLine
					+ Environment.NewLine
					+ HelpSubCommandForMoreInformation;
        }

        public bool CanHelp(string val=null)
        {
            return string.IsNullOrEmpty(val) 
                ? _classAndMethodRecognizers.Any(cmr => cmr.Type != typeof(HelpController)) 
                : _classAndMethodRecognizers.Any(cmr=>cmr.ClassName().Equals(val,StringComparison.OrdinalIgnoreCase));
        }
    }
}
