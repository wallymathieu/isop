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
            ParserBuilder.Dispose();
            base.OnExit(e);
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var path = Directory.GetParent(System.Reflection.Assembly.GetExecutingAssembly().Location)
                .FullName;
            ParserBuilder = new Build().ConfigurationFrom(
                path);
        }

        public Build ParserBuilder { get; set; }
    }
}
