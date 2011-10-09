using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
namespace Isop.Console
{
    public class HelpController
    {
        HelpForArgumentWithOptions helpForArgumentWithOptions;
        HelpForClassAndMethod helpForClassAndMethod;

        public HelpController(
            HelpForArgumentWithOptions helpForArgumentWithOptions, 
            HelpForClassAndMethod helpForClassAndMethod)
        {
            this.helpForArgumentWithOptions = helpForArgumentWithOptions;
            this.helpForClassAndMethod = helpForClassAndMethod;
        }

        public string Index()
        {
            var sb = new StringBuilder();
            if (helpForArgumentWithOptions.CanHelp())
            {
              sb.AppendLine(helpForArgumentWithOptions.Help());
            }
            if (helpForClassAndMethod.CanHelp())
            {
                sb.AppendLine(helpForClassAndMethod.Help());
            }
            return sb.ToString().Trim(' ','\t','\r','\n');
        }
		public string Index(string command)
        {
			if (String.IsNullOrEmpty(command))
				return Index();
            var sb = new StringBuilder();
            if (helpForArgumentWithOptions.CanHelp(command))
            {
              sb.AppendLine(helpForArgumentWithOptions.Help(command));
            }
            if (helpForClassAndMethod.CanHelp(command))
            {
                sb.AppendLine(helpForClassAndMethod.Help(command));
            }
            return sb.ToString().Trim(' ','\t','\r','\n');
        }
    }

    public class HelpForArgumentWithOptions
    {
        readonly IEnumerable<ArgumentWithOptions> argumentWithOptionses;
        public string TheArgumentsAre { get; set; }

        public HelpForArgumentWithOptions(IEnumerable<ArgumentWithOptions> argumentWithOptionses)
        {
            this.argumentWithOptionses = argumentWithOptionses;
            TheArgumentsAre = "The arguments are:";
        }

        public string Help(string val=null)
        {
			if (string.IsNullOrEmpty(val))
	            return TheArgumentsAre + Environment.NewLine +
                   String.Join(Environment.NewLine,
                               argumentWithOptionses.Select(ar => "  " + ar.Help()).ToArray());
        	return argumentWithOptionses.First(ar=>ar.Argument.Prototype.Equals(val)).Help();
		}

        public bool CanHelp(string val=null)
        {
            if (string.IsNullOrEmpty(val)) return argumentWithOptionses.Any();
			return argumentWithOptionses.Any(ar=>ar.Argument.Prototype.Equals(val));
        }
    }
    public class HelpForClassAndMethod
    {
        readonly IEnumerable<ClassAndMethodRecognizer> classAndMethodRecognizers;
        private readonly TypeContainer container;
        public string TheCommandsAre { get; set; }
		public string TheSubCommandsFor { get; set; }
        public string HelpCommandForMoreInformation { get; set; }

        public string HelpSubCommandForMoreInformation { get; set; }

        public HelpForClassAndMethod(IEnumerable<ClassAndMethodRecognizer> classAndMethodRecognizers, TypeContainer container)
        {
            this.container=container;
            this.classAndMethodRecognizers = classAndMethodRecognizers;

            HelpSubCommandForMoreInformation = "Se 'COMMANDNAME' help <command> <subcommand> for more information";

            HelpCommandForMoreInformation = "Se 'COMMANDNAME' help <command> for more information";
            TheCommandsAre = "The commands are:";
			TheSubCommandsFor = "The sub commands for ";
        }
        private string _Description(Type t,MethodInfo method=null)
        { 
            var description = t.GetMethods().FirstOrDefault(m=>
                m.Name.Equals("help",StringComparison.OrdinalIgnoreCase));
            //TODO: should match parameters to make sure that it can accept 1 param of type string
            if (null==description) return string.Empty;
            
            var obj = container.CreateInstance(t);
            
            return "  "+description.Invoke(obj,new[]{(method!=null? method.Name:null)});
        }
        private string _Help(ClassAndMethodRecognizer cmr,bool simpleDescription)
        {
            if (simpleDescription) return cmr.ClassName()+ _Description(cmr.Type);
            
            return cmr.ClassName()
                +Environment.NewLine
                +Environment.NewLine
                +String.Join(Environment.NewLine, cmr.GetMethods()
                        .Select(m=>"  "+m.Name+_Description(cmr.Type,m)).ToArray());
        }
        
        public string Help(string val=null)
        {
            if (string.IsNullOrEmpty(val)) return TheCommandsAre + Environment.NewLine +
                   String.Join(Environment.NewLine,
                               classAndMethodRecognizers.Select(cmr => "  " + _Help(cmr,true)).ToArray())
                   + Environment.NewLine
                   + Environment.NewLine
                   + HelpCommandForMoreInformation;
			
			return TheSubCommandsFor+
				_Help(classAndMethodRecognizers.First(cmr=>cmr.ClassName().Equals(val)),false)
					+ Environment.NewLine
					+ Environment.NewLine
					+ HelpSubCommandForMoreInformation;
        }

        public bool CanHelp(string val=null)
        {
            if (string.IsNullOrEmpty(val)) return classAndMethodRecognizers.Any();
			return classAndMethodRecognizers.Any(cmr=>cmr.ClassName().Equals(val));
        }
    }
}
