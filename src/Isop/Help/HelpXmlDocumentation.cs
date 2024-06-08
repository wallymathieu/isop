using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;

namespace Isop.Help
{
    internal sealed class HelpXmlDocumentation
    {
        private static Dictionary<string, string> GetSummariesFromText(string text)
        {
            var xml = new XmlDocument();
            xml.LoadXml(text);
            var members = xml.GetElementsByTagName("members");
            var member = members.Item(0)?.ChildNodes;
            Dictionary<string, string> doc = new();
            if (member is not null)
                foreach (XmlNode m in member)
                {
                    var attr = m.Attributes;
                    if (attr is null) continue;
                    var name = attr.GetNamedItem("name");
                    if (name is null) continue;
                    var nodes = m.ChildNodes.Cast<XmlNode>();
                    var summary = nodes.FirstOrDefault(x => x.Name.Equals("summary", StringComparison.Ordinal));
                    if (null != summary)
                        doc.Add(name.InnerText, summary.InnerText.Trim());
                }
            return doc;
        }
        private readonly Dictionary<Assembly, IDictionary<string, string>> _summaries = [];
        private IDictionary<string, string> GetAssemblyCachedSummaries(Assembly a)
        {
            if (_summaries.TryGetValue(a, out var value)) return value;
            else
            {
                var file = TryGetAssemblyLocation(a);
                _summaries.Add(a, null != file
                    ? GetSummariesFromText(File.ReadAllText(file))
                    : []);

                return _summaries[a];
            }
        }

        private static string? TryGetAssemblyLocation(Assembly a)
        {
            var loc = a.Location;
            string path = new Uri(a.Location).AbsolutePath;
            var paths = new[] { HelpAt(loc, loc), HelpAt(path, path), HelpAt(path, loc), HelpAt(loc, path) };
            var file = paths.FirstOrDefault(File.Exists);
            return file;
        }

        private static string HelpAt(string path, string filename)
        {
            return Path.Combine(Path.GetDirectoryName(path)!,
                                Path.GetFileNameWithoutExtension(filename) + ".xml");
        }

        public static string GetKey(MethodInfo method)
        {
            return GetKey(method.DeclaringType!, method);
        }
        private static readonly Regex GetOrSet = new Regex("^(get|set)_", RegexOptions.IgnoreCase);

        private static string GetKey(Type t, MethodInfo method)
        {
            if (GetOrSet.IsMatch(method.Name))
                return string.Format(CultureInfo.InvariantCulture, "P:{0}.{1}", GetFullName(t), method.Name.Substring(4));
            var parameters = method.GetParameters();
            return string.Format(CultureInfo.InvariantCulture, "M:{0}.{1}{2}", GetFullName(t), method.Name, parameters.Length != 0 ? string.Concat("(", TypeNames(parameters), ")") : "");
        }

        private static string TypeNames(ParameterInfo[] parameters)
        {
            return string.Join(",", parameters.Select(p => p.ParameterType.FullName).ToArray());
        }

        private static string GetKey(Type t)
        {
            return "T:" + GetFullName(t);
        }
        private static string GetFullName(Type t)
        {
            if (t.FullName is null) throw new Exception($"Missing fullname for type! {t.Name}");
            return t.FullName.Replace("+", ".");
        }
        public string GetDescriptionForMethod(MethodInfo method)
        {
            var t = method.DeclaringType;
            var summaries = GetAssemblyCachedSummaries(t!.GetTypeInfo().Assembly);
            var key = GetKey(t!, method);
            if (summaries.TryGetValue(key, out string? value))
                return value;
            return string.Empty;
        }
        public string GetDescriptionForType(Type t)
        {
            var summaries = GetAssemblyCachedSummaries(t.GetTypeInfo().Assembly);
            var key = GetKey(t);
            if (summaries.TryGetValue(key, out string? value))
                return value;
            return string.Empty;
        }
    }
}

