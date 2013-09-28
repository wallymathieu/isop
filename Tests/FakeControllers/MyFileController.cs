using System;
using System.IO;

namespace Isop.Tests
{
    class MyFileController
    {
        public Func<FileStream, string> OnAction { get; set; }
        public string Action(FileStream file) { return OnAction(file); }
    }
}