using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.IO;
using Isop.Infrastructure;
using System.Text.RegularExpressions;

namespace Isop.Help
{
    public class HelpXmlDocumentation
    {
        public IDictionary<string, string> GetSummariesFromText(string text)
        {
            var xml = new System.Xml.XmlDocument();
            xml.LoadXml(text);
            var members = xml.GetElementsByTagName("members");
            var member = members.Item(0).ChildNodes;
            Dictionary<string, string> doc = new Dictionary<string, string>();
            foreach (System.Xml.XmlNode m in member)
            {
                var attr = m.Attributes;
                var name = attr.GetNamedItem("name");
                var nodes = m.ChildNodes.Cast<System.Xml.XmlNode>();
                var summary = nodes.FirstOrDefault(x => x.Name.Equals("summary"));
                if (null != summary)
                    doc.Add(name.InnerText, summary.InnerText.Trim());
            }
            return doc;
        }
        private Dictionary<Assembly, IDictionary<string, string>> summaries = new Dictionary<Assembly, IDictionary<string, string>>();
        public IDictionary<string, string> GetSummariesForAssemblyCached(Assembly a)
        {
            if (summaries.ContainsKey(a)) return summaries[a];
            else
            {
                var loc = a.Location;
                string path = new Uri(Assembly.GetExecutingAssembly().CodeBase).AbsolutePath;
                var paths = new[] { HelpAt(loc, loc), HelpAt(path, path), HelpAt(path, loc), HelpAt(loc, path) };
                var file = paths.FirstOrDefault(f => File.Exists(f));
                if (null != file)
                {
                    summaries.Add(a, GetSummariesFromText(File.ReadAllText(file)));
                }
                else
                {
                    summaries.Add(a, new Dictionary<string, string>()); // 
                }

                return summaries[a];
            }
        }

        private static string HelpAt(string path, string filename)
        {
            return Path.Combine(Path.GetDirectoryName(path),
                                Path.GetFileNameWithoutExtension(filename) + ".xml");
        }

        public string GetKey(MethodInfo method)
        {
            return GetKey(method.DeclaringType, method);
        }
        private static Regex _getOrSet = new Regex("^(get|set)_", RegexOptions.IgnoreCase);
        public string GetKey(Type t, MethodInfo method)
        {
            if (_getOrSet.IsMatch(method.Name))
                return "P:" + GetFullName(t) + "." + method.Name.Substring(4);
            var parameters = method.GetParameters();
            return "M:" + GetFullName(t) + "." + method.Name + (parameters.Any() ? "(" + TypeNames(parameters) + ")" : "");
        }

        private string TypeNames(ParameterInfo[] parameters)
        {
            return String.Join(",", parameters.Select(p => p.ParameterType.FullName).ToArray());
        }
        public string GetKey(Type t)
        {
            return "T:" + GetFullName(t);
        }
        private string GetFullName(Type t)
        {
            return t.FullName.Replace("+", ".");
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
}

