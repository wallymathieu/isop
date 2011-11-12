using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
namespace Isop
{
    public class HelpXmlDocumentation
    {
        public IDictionary<string, string> GetSummariesFromText (string text)
        {
            var xml = new System.Xml.XmlDocument();
            xml.LoadXml(text);
            var members = xml.GetElementsByTagName("members");
            var member = members.Item(0).ChildNodes;
            Dictionary<string,string> doc = new Dictionary<string, string>();
            foreach (System.Xml.XmlNode m in member)
            {
                var attr = m.Attributes;
                var name = attr.GetNamedItem("name");
                var nodes = m.ChildNodes.Cast<System.Xml.XmlNode>();
                var summary = nodes.FirstOrDefault(x=>x.Name.Equals("summary"));
                if (null!=summary)
                    doc.Add(name.InnerText,summary.InnerText.Trim());
            }
            return doc;
        }
        private Dictionary<Assembly,IDictionary<string,string>> summaries = new Dictionary<Assembly, IDictionary<string, string>>(); 
        public IDictionary<string,string> GetSummariesForAssemblyCached(Assembly a)
        {
            if (summaries.ContainsKey(a)) return summaries[a];
            else {
                var loc= a.Location;
                var xmlDocFile = Path.Combine(Path.GetDirectoryName(loc),
                    Path.GetFileNameWithoutExtension(loc)+".xml");
                if (File.Exists(xmlDocFile)){
                    summaries.Add(a,GetSummariesFromText(File.ReadAllText(xmlDocFile)));
                }else{
                    summaries.Add(a,new Dictionary<string,string>());// 
                }
                return summaries[a];
            }
        }
        public string GetKey(MethodInfo method)
        {
           return  GetKey(method.DeclaringType,method);
        }
        public string GetKey(Type t,MethodInfo method)
        {
            if (method.Name.StartsWith("get_",StringComparison.OrdinalIgnoreCase)
                || method.Name.StartsWith("set_",StringComparison.OrdinalIgnoreCase))
                return "P:"+GetFullName(t)+"."+method.Name.Substring(4);
            return "M:"+GetFullName(t)+"."+method.Name;
        }
        public string GetKey(Type t)
        {
            return "T:"+GetFullName(t);
        }
        private string GetFullName(Type t)
        {
             return t.FullName.Replace("+",".");
        }
        public string GetDescriptionForMethod(MethodInfo method)
        {
            var t = method.DeclaringType;
            var summaries = GetSummariesForAssemblyCached(t.Assembly);
            var key = GetKey(t, method);
            if (summaries.ContainsKey(key)) 
                return summaries[key];
            return string.Empty;
        }
        public string GetDescriptionForType(Type t)
        {
            var summaries = GetSummariesForAssemblyCached(t.Assembly);
            var key = GetKey(t);
            if (summaries.ContainsKey(key)) 
                return summaries[key];
            return string.Empty;
        }

    }
    
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
        private readonly MethodInfoFinder methodInfoFinder = new MethodInfoFinder();
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
        private string Description(Type t,MethodInfo method=null)
        { 
            var description = methodInfoFinder.Match(t.GetMethods(),
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
