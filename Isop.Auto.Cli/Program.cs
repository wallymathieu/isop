using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Isop.Auto.Cli
{
    class Program
    {

        static void Main(string[] args)
        {
            Console.WriteLine(Environment.CurrentDirectory);
            Console.WriteLine(System.Reflection.Assembly.GetExecutingAssembly().Location);

        }
    }
}
