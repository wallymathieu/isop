using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Isop.Infrastructure;
using System.Runtime.Loader;
namespace Isop.Infrastructure
{
    public class LoadAssemblies
    {
        public IEnumerable<Assembly> LoadFrom(string path)
        {
            var files = Directory.GetFiles(path)
                .Where(f =>
                {
                    var ext = Path.GetExtension(f);
                    return ext.EqualsIgnoreCase(".dll") || ext.EqualsIgnoreCase(".exe");
                })
                .Where(f => !Path.GetFileNameWithoutExtension(f).StartsWithIgnoreCase("Isop"));
            
            foreach (var file in files)
            {
                var assembly =  AssemblyLoadContext.Default.LoadFromAssemblyPath(file);
                yield return assembly;
            }
        }
    }

}