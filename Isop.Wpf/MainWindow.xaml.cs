using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using Isop.WpfControls.ViewModels;
using System.Collections.Generic;
using System.ComponentModel;
using Isop.WpfControls;

namespace Isop.Gui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MethodTreeModel MethodTreeModel { get; set; }

        public MainWindow()
        {
            MethodTreeModel = ((App)App.Current).ParserBuilder.GetMethodTreeModel();
            InitializeComponent();
            paramview.Source = MethodTreeModel.GlobalParameters;
            textBlock2.Text = string.Empty;
            controllersAndCommands.DataContext = MethodTreeModel.Controllers;
            textBlock1.Text = string.Empty;
        }

        private void SelectedMethodChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is Method)
            {
                MethodTreeModel.CurrentMethod = (Method)e.NewValue;
                methodview.DataContext = e.NewValue;
                methodview.Source = MethodTreeModel.CurrentMethod.Parameters;
                textBlock2.Text = MethodTreeModel.CurrentMethod.Help;
                textBlock1.Text = string.Empty;
            }
        }

        private void ExecuteMethodButtonClick(object sender, RoutedEventArgs e)
        {
            var cout = new StringWriter(CultureInfo.CurrentCulture);
            try
            {
                MethodTreeModel.GlobalParameters.GetParsedArguments().Invoke(cout);
            }
            catch (MissingArgumentException ex)
            {
                textBlock1.Text = String.Format("Missing argument(s): {0}", String.Join(", ", ex.Arguments.Select(a => String.Format("{0}: {1}", a.Key, a.Value)).ToArray()));
                return;
            }
#if DEBUG
            catch (Exception ex1)
            {
                textBlock1.Text = string.Join(Environment.NewLine, new object[]{ 
                    "The global parameter invokation failed with exception:",
                    ex1.Message, ex1.StackTrace});
                return;
            }
#endif
            if (null == MethodTreeModel.CurrentMethod) return;

            cout.WriteLine();
            try
            {
                var parsedMethod = MethodTreeModel.Build.Parse(MethodTreeModel.CurrentMethod);
                parsedMethod.Invoke(cout);
            }
            catch (MissingArgumentException ex)
            {
                textBlock1.Text = String.Format("Missing argument(s): {0}", String.Join(", ", ex.Arguments.Select(a => String.Format("{0}: {1}", a.Key, a.Value)).ToArray()));
                return;
            }
#if DEBUG
            catch (Exception ex1)
            {
                textBlock1.Text = string.Join(Environment.NewLine, new object[] { 
                    "The invokation of the action failed with exception:", 
                    ex1.Message, ex1.StackTrace });
                return;
            }
#endif
            textBlock1.Text = cout.ToString();
        }


    }
}