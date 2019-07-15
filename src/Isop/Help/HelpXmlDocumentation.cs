using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Linq;
using System.IO;
using Isop.Infrastructure;
using System.Text.RegularExpressions;
using System.Xml;

namespace Isop.Help
{
    internal class HelpXmlDocumentation
    {
        public static IDictionary<string, string> GetSummariesFromText(string text)
        {
            var xml = new XmlDocument();
            xml.LoadXml(text);
            var members = xml.GetElementsByTagName("members");
            var member = members.Item(0).ChildNodes;
            Dictionary<string, string> doc = new Dictionary<string, string>();
            foreach (XmlNode m in member)
            {
                var attr = m.Attributes;
                var name = attr.GetNamedItem("name");
                var nodes = m.ChildNodes.Cast<XmlNode>();
                var summary = nodes.FirstOrDefault(x => x.Name.Equals("summary"));
                if (null != summary)
                    doc.Add(name.InnerText, summary.InnerText.Trim());
            }
            return doc;
        }
        private readonly Dictionary<Assembly, IDictionary<string, string>> _summaries = new Dictionary<Assembly, IDictionary<string, string>>();
        private IDictionary<string, string> GetAssemblyCachedSummaries(Assembly a)
        {
            if (_summaries.ContainsKey(a)) return _summaries[a];
            else
            {
                var file = TryGetAssemblyLocation(a);
                _summaries.Add(a, null != file
                    ? GetSummariesFromText(File.ReadAllText(file))
                    : new Dictionary<string, string>());

                return _summaries[a];
            }
        }

        private static string TryGetAssemblyLocation(Assembly a)
        {
            var loc = a.Location;
            string path = new Uri(a.CodeBase).AbsolutePath;
            var paths = new[] {HelpAt(loc, loc), HelpAt(path, path), HelpAt(path, loc), HelpAt(loc, path)};
            var file = paths.FirstOrDefault(File.Exists);
            return file;
        }

        private static string HelpAt(string path, string filename)
        {
            return Path.Combine(Path.GetDirectoryName(path),
                                Path.GetFileNameWithoutExtension(filename) + ".xml");
        }

        public static string GetKey(MethodInfo method)
        {
            return GetKey(method.DeclaringType, method);
        }
        private static readonly Regex GetOrSet = new Regex("^(get|set)_", RegexOptions.IgnoreCase);
        public static string GetKey(Type t, MethodInfo method)
        {
            if (GetOrSet.IsMatch(method.Name))
                return string.Format(CultureInfo.InvariantCulture, "P:{0}.{1}", GetFullName(t), method.Name.Substring(4));
            var parameters = method.GetParameters();
            return string.Format(CultureInfo.InvariantCulture, "M:{0}.{1}{2}", GetFullName(t), method.Name, (parameters.Any() ? string.Concat("(", TypeNames(parameters), ")") : ""));
        }

        private static string TypeNames(ParameterInfo[] parameters)
        {
            return string.Join(",", parameters.Select(p => p.ParameterType.FullName).ToArray());
        }
        public static string GetKey(Type t)
        {
            return "T:" + GetFullName(t);
        }
        private static string GetFullName(Type t)
        {
            return t.FullName.Replace("+", ".");
        }
        public string GetDescriptionForMethod(MethodInfo method)
        {
            var t = method.DeclaringType;
            var summaries = GetAssemblyCachedSummaries(t.GetTypeInfo().Assembly);
            var key = GetKey(t, method);
            if (summaries.ContainsKey(key))
                return summaries[key];
            return string.Empty;
        }
        public string GetDescriptionForType(Type t)
        {
            var summaries = GetAssemblyCachedSummaries(t.GetTypeInfo().Assembly);
            var key = GetKey(t);
            if (summaries.ContainsKey(key))
                return summaries[key];
            return string.Empty;
        }
    }
}

