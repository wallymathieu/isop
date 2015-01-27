using System.Collections.Generic;
using System;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using Isop.Client.Json;
using Isop.Client;
using Isop.Gui.Adapters;
namespace Isop.Gui.ViewModels
{
    public class RootViewModel
    {
        public RootViewModel(IClient isopClient = null, Isop.Client.Models.Root root = null)
        {
            this.isopClient = isopClient;
            this.root = root = root ?? new Isop.Client.Models.Root();
            GlobalParameters = new ObservableCollection<ParamViewModel>((root.GlobalParameters ?? new Isop.Client.Models.Param[0]).Select(Map));
            Controllers = new ObservableCollection<ControllerViewModel>((root.Controllers ?? new Isop.Client.Models.Controller[0]).Select(Map));
            singleEventHandlerScope = new SingleScopeOnly();
            GlobalParameters.RegisterPropertyChanged(globalValueChanged);
        }


        public ObservableCollection<ParamViewModel> GlobalParameters { get; private set; }
        public ObservableCollection<ControllerViewModel> Controllers { get; private set; }
        /// <summary>
        /// ignore changes if within another scope
        /// </summary>
        private SingleScopeOnly singleEventHandlerScope;
        private MethodViewModel _currentMethod;
        private IClient isopClient;
        private Isop.Client.Models.Root root;
        public MethodViewModel CurrentMethod
        {
            get { return _currentMethod; }
            set
            {
                if (null != _currentMethod)
                {
                    _currentMethod.Parameters.DeRegisterPropertyChanged(methodValueChanged);
                }
                _currentMethod = value;
                if (null != _currentMethod)
                {
                    foreach (var param in GlobalParameters)
                    {
                        CurrentMethod.Parameters.SetParamValueOnMatching(param);
                    }
                    _currentMethod.Parameters.RegisterPropertyChanged(methodValueChanged);
                }
            }
        }

        private void globalValueChanged(object sender, PropertyChangedEventArgs args)
        {
            singleEventHandlerScope.Try(() =>
            {
                if (null != CurrentMethod)
                {
                    CurrentMethod.Parameters.SetParamValueOnMatching(((ParamViewModel)sender));
                }
            });
        }
        private void methodValueChanged(object sender, PropertyChangedEventArgs args)
        {
            singleEventHandlerScope.Try(() =>
            {
                GlobalParameters.SetParamValueOnMatching(((ParamViewModel)sender));
            });
        }

        public void Accept(Isop.Client.Models.Root res)
        {
            root = res;
            foreach (var pvm in res.GlobalParameters.Select(Map))
            {
                GlobalParameters.Add(pvm);
            }
            foreach (var cvm in res.Controllers.Select(Map))
            {
                Controllers.Add(cvm);
            }
        }

        private ControllerViewModel Map(Isop.Client.Models.Controller c)
        {
            return new Isop.Gui.ViewModels.ControllerViewModel()
            {
                Name = c.Name,
                Methods = c.Methods.Select(m =>
                    new MethodViewModel(m)).ToArray()
            };
        }

        private static ParamViewModel Map(Isop.Client.Models.Param p)
        {
            return new ParamViewModel(p);
        }

        public async Task<MethodViewModel> Execute()
        {
            await isopClient.Invoke(root, CurrentMethod.Method, CurrentMethod);
            return CurrentMethod;
        }

        private void Clear()
        {
            this.root = new Isop.Client.Models.Root();
            GlobalParameters.Clear();
            Controllers.Clear();
        }

        public IClient Client
        {
            get
            {
                return isopClient;
            }
            set
            {
                isopClient = value;
                Clear();
            }
        }
    }
}