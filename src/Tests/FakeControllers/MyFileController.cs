using System;
using System.IO;

namespace Isop.Tests.FakeControllers
{
    class MyFileController
    {
        public Func<FileStream, string> OnAction { get; set; }
        public string Action(FileStream file) { return OnAction(file); }
    }
}