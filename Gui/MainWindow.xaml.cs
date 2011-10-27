using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace Isop.Gui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MethodTreeModel MethodTreeModel { get; set; }
        public ObservableCollection<Param> Parameters;
        public Method CurrentMethod { get; set; }
        protected ArgumentParserBuilder ParserBuilder { get; set; }

        public MainWindow()
        {
            //Console.SetOut
            ParserBuilder = ArgumentParser.Build()
                     .Parameter("global")
                     .RecognizeHelp()
                     .Recognize(typeof(MyController))
                     .Recognize(typeof(CustomerController));

            // TODO:
            MethodTreeModel = new MethodTreeModel
                                    {
                                        Controllers = ParserBuilder.GetControllerRecognizers()
                                            .Where(cmr=>! cmr.ClassName().Equals("help",StringComparison.OrdinalIgnoreCase))
                                            .Select(cmr => new Controller()
                                            {
                                                Name = cmr.ClassName(),
                                                Methods = cmr.GetMethods().Select(m => new Method(m.Name, cmr.ClassName())
                                                    {
                                                        Parameters = new ObservableCollection<Param>(
                                                            m.GetParameters().Select(p => new Param(p.ParameterType, p.Name)))
                                                    })
                                            }),
                                        GlobalParameters = new ObservableCollection<Param> (ParserBuilder.GetGlobalParameters().Select(p=>new Param(typeof(string),p.Argument.ToString())))
                                    };

            InitializeComponent();
            paramview.Parameters = MethodTreeModel.GlobalParameters;
            controllersAndCommands.DataContext = MethodTreeModel.Controllers;
            textBlock1.Text = string.Empty;
        }


        private void SelectedMethodChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is Method)
            {
                CurrentMethod = (Method) e.NewValue;
                methodview.DataContext = e.NewValue;
                methodview.Parameters = CurrentMethod.Parameters;
                textBlock1.Text = string.Empty;
            }
        }

        private void ExecuteMethodButtonClick(object sender, RoutedEventArgs e)
        {
            //TODO: need to refactor this
            var consoleIn = new List<string>() { CurrentMethod.ClassName, CurrentMethod.Name };
            consoleIn.AddRange(CurrentMethod.Parameters
                .Where(p=>!string.IsNullOrWhiteSpace(p.Value))
                .Select(p => string.Format("--{0}=\"{1}\"", p.Name, p.Value)));
            consoleIn.AddRange(MethodTreeModel.GlobalParameters
                .Where(p => !string.IsNullOrWhiteSpace(p.Value))
                .Select(p => string.Format("--{0}=\"{1}\"", p.Name, p.Value)));

            textBlock1.Text = ParserBuilder.Parse(consoleIn).Invoke();
        }
    }
}
