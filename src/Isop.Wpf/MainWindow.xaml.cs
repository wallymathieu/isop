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
using Isop.Gui.Adapters;
using Isop.Infrastructure;

namespace Isop.Gui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public RootViewModel MethodTreeModel { get; set; }
        private SelectClient selectClient;

        public MainWindow()
        {
            MethodTreeModel = new RootViewModel();
            InitializeComponent();
            selectClient = new SelectClient();
            selectClient.Source.Loaded += Source_Loaded;
            var empty = new EmptyMethodViewModel();
            ResultBlock.DataContext = empty;
            HelpBlock.DataContext = empty;
            paramview.Source = MethodTreeModel.GlobalParameters;
            controllersAndCommands.DataContext = MethodTreeModel.Controllers;

            var conn = new IsopClient(new JsonHttpClient(), "http://localhost:8080/");
            InitFromClient(new JsonClient(conn), _ =>
            {
                var assemblies = new LoadAssemblies().LoadFrom(ExecutionAssembly.Path()).ToArray();
                var build = new Build();
                foreach (var assembly in assemblies)
                {
                    build.ConfigurationFrom(assembly);
                }
                InitFromClient(new BuildClient(build));
            });
        }

        private void InitFromClient(IClient client, Action<AggregateException> onfailure = null)
        {
            var model = client.GetModel();
            MethodTreeModel.Client = client;
            AsyncLoad(model, onfailure);
        }

        void Source_Loaded()
        {
            var client = selectClient.Source.GetClient();
            if (client != null)
            {
                MethodTreeModel.Client = client;
                var model = client.GetModel();
                AsyncLoad(model);
            }
        }

        private void AsyncLoad(Task<Client.Models.Root> model, Action<AggregateException> onfailure = null)
        {
            model.ContinueWith((t) =>
                {
                    if (t.Exception != null)
                    {
                        if (onfailure != null) { onfailure(t.Exception); }
                    }
                    else
                    {
                        MethodTreeModel.Accept(t.Result);
                    }
                }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void SelectClient(object sender, RoutedEventArgs e)
        {
            selectClient.Show();
        }

        private void SelectedMethodChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is MethodViewModel)
            {
                MethodTreeModel.CurrentMethod = (MethodViewModel)e.NewValue;
                methodview.DataContext = e.NewValue;
                methodview.Source = MethodTreeModel.CurrentMethod.Parameters;
                ResultBlock.DataContext = MethodTreeModel.CurrentMethod;
                HelpBlock.DataContext = MethodTreeModel.CurrentMethod;
            }
            else
            {
                var empty = new EmptyMethodViewModel();
                ResultBlock.DataContext = empty;
                HelpBlock.DataContext = empty;
            }
        }

        private async void ExecuteMethodButtonClick(object sender, RoutedEventArgs e)
        {
            if (null == MethodTreeModel.CurrentMethod) return;

            await MethodTreeModel.Execute();
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            selectClient.Show();
            selectClient.Window_Drop(sender, e);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            selectClient.Close();
        }
    }
}