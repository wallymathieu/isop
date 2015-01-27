using System;
using System.IO;
using System.Reflection;
namespace Isop.Infrastructure
{
    public class ExecutionAssembly
    {
        public static string Path()
        {
            return Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName;
        }
    }
}

