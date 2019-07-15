using System;
using System.IO;
using System.Linq;
using Isop;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class Given_controller_with_file_argument
    {
        class MyFileController
        {
            public Func<FileStream, string> OnAction { get; set; }
            public string Action(FileStream file) { return OnAction(file); }
        }
        [Test]
        public void It_can_handle_file_argument()
        {
            var filename = string.Empty;

            var sc = new ServiceCollection();
            sc.AddSingleton(ci => new MyFileController() { OnAction = (file) => filename = file.Name });


            FileStream fileStream = null;
            try
            {

                var arguments = Builder.Create(sc)
                        
                    .SetTypeConverter((t, s, c) =>
                    {

                        fileStream = new FileStream(s, FileMode.Create);
                        return fileStream;
                    })
                    //Need to set type converter 
                    .Recognize(typeof(MyFileController))
                    .BuildAppHost()
                    .Parse(new[] { "MyFile", "Action", "--file", "myfile.txt" });

                Assert.That(arguments.Unrecognized.Count(), Is.EqualTo(0));
                arguments.Invoke(new StringWriter());
                Assert.True(filename.Contains("myfile.txt"));
            }
            finally
            {
                if (null != fileStream)
                {
                    try
                    {
                        fileStream.Dispose();
                    }
                    // ReSharper disable EmptyGeneralCatchClause
                    catch { }
                    // ReSharper restore EmptyGeneralCatchClause
                }
            }
        }

    }
}