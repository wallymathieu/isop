using System;
using System.IO;
using System.Reflection;
namespace Isop.Infrastructure
{
    internal class ExecutionAssembly
    {
        public static string Path()
        {
            return Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName;
        }
    }
}

