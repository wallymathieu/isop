﻿using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Isop.Infrastructure;
#if NETSTANDARD1_6
using System.Runtime.Loader;
#endif
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
var assembly = 
#if NETSTANDARD1_6
                AssemblyLoadContext.Default.LoadFromAssemblyPath(file);
#else
                Assembly.LoadFile(file);
#endif
                yield return assembly;
            }
        }
    }

}