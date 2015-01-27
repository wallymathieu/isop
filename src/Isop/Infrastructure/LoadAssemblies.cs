using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Isop.Infrastructure;

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
                    return ext.EqualsIC(".dll") || ext.EqualsIC(".exe");
                })
                .Where(f => !Path.GetFileNameWithoutExtension(f).StartsWithIC("Isop"));
            
            foreach (var file in files)
            {
                var assembly = Assembly.LoadFile(file);
                yield return assembly;
            }
        }
    }

}