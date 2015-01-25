using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using Isop.Gui.ViewModels;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Isop.Client;
using Isop.Client.Json;

namespace Isop.Gui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public RootViewModel MethodTreeModel { get; set; }

        public MainWindow()
        {
            var conn = new IsopClient(new JsonHttpClient(), "http://localhost:8080/");

            MethodTreeModel = new RootViewModel(conn);
            InitializeComponent();
            var empty = new EmptyMethodViewModel();
            textBlock2.DataContext = empty;
            textBlock1.DataContext = empty;
            paramview.Source = MethodTreeModel.GlobalParameters;
            controllersAndCommands.DataContext = MethodTreeModel.Controllers;

            var model = conn.GetModel();
            model.ContinueWith((t) =>
                MethodTreeModel.Accept(t.Result),
                TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void SelectedMethodChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is MethodViewModel)
            {
                MethodTreeModel.CurrentMethod = (MethodViewModel)e.NewValue;
                methodview.DataContext = e.NewValue;
                methodview.Source = MethodTreeModel.CurrentMethod.Parameters;
                textBlock2.DataContext = MethodTreeModel.CurrentMethod;
                textBlock1.DataContext = MethodTreeModel.CurrentMethod;
            }
            else
            {
                var empty = new EmptyMethodViewModel();
                textBlock2.DataContext = empty;
                textBlock1.DataContext = empty;
            }
        }

        private async void ExecuteMethodButtonClick(object sender, RoutedEventArgs e)
        {
            if (null == MethodTreeModel.CurrentMethod) return;

            await MethodTreeModel.Execute();
        }
    }
}