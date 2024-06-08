using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Isop;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Tests.TypeConversions
{
    [TestFixture]
    public class Given_controller_with_file_argument
    {
        class MyFileController(Func<FileStream, string> OnAction)
        {
            public string Action(FileStream file) { return OnAction(file); }
        }

        class DeleteOnDispose:IDisposable
        {
            public List<FileStream> files { get; }=new List<FileStream>();

            public void Dispose()
            {
                foreach (var fileStream in files)
                {
                    var name = fileStream.Name;
                    try
                    {
                        fileStream.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"Tried to dispose fileStream {ex.Message}");
                    }

                    if (!File.Exists(name)) continue;
                    try
                    {
                        File.Delete(name);
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"Tried to delete file {ex.Message}");
                    }
                }
                files.Clear();
            }
        }
        
        [Test]
        public async Task It_can_handle_file_argument()
        {
            var filename = string.Empty;
            var sc = new ServiceCollection();
            sc.AddSingleton(ci => new MyFileController (OnAction: file => filename = file.Name ));
            using (var files = new DeleteOnDispose())
            {
                var arguments = AppHostBuilder.Create(sc)
                    .SetTypeConverter((t, s, c) =>
                    {
                        //creating file before use ...
                        var fileStream = new FileStream(s!, FileMode.Create);
                        files.files.Add(fileStream);
                        return fileStream;
                    })
                    .Recognize(typeof(MyFileController))
                    .BuildAppHost()
                    .Parse(["MyFile", "Action", "--file", "myfile.txt"]);

                Assert.That(arguments.Unrecognized.Select(u => u.Value).ToArray(), Is.Empty);
                await arguments.InvokeAsync(new StringWriter());
                Assert.That(filename.Contains("myfile.txt"), Is.True);
            }
        }

    }
}