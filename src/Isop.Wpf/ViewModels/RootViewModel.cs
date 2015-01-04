using System.Collections.Generic;
using System;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
namespace Isop.Gui.ViewModels
{
    public class RootViewModel
    {
        public RootViewModel(Gui.IIsopClient isopClient, IEnumerable<Gui.Models.Param> globalParameters = null, IEnumerable<Gui.Models.Controller> controllers = null)
        {
            this.isopClient = isopClient;
            GlobalParameters = new ObservableCollection<ParamViewModel>((globalParameters ?? new Gui.Models.Param[0]).Select(Map));
            Controllers = new ObservableCollection<ControllerViewModel>((controllers ?? new Gui.Models.Controller[0]).Select(Map));
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
        private System.Threading.Tasks.Task<Gui.Models.Root> task;
        private Gui.IIsopClient isopClient;
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

        public void Accept(Gui.Models.Root res)
        {
            foreach (var pvm in res.GlobalParameters.Select(Map))
            {
                GlobalParameters.Add(pvm);
            }
            foreach (var cvm in res.Controllers.Select(Map))
            {
                Controllers.Add(cvm);
            }
        }

        private ControllerViewModel Map(Gui.Models.Controller c)
        {
            return new Isop.Gui.ViewModels.ControllerViewModel()
            {
                Name = c.Name,
                Methods = c.Methods.Select(m =>
                    new MethodViewModel(m, isopClient)).ToArray()
            };
        }

        private static ParamViewModel Map(Gui.Models.Param p)
        {
            return new ParamViewModel(p);
        }
    }
}