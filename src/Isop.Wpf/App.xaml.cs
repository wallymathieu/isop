using Isop.Client;
using Isop.Client.Json;
using System.IO;
using System.Windows;

namespace Isop.Gui
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ClientConnection = new IsopClient(new JsonHttpClient(), "http://localhost:8080/");
        }

        public IsopClient ClientConnection { get; set; }
    }
}
