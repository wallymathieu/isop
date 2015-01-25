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
namespace Isop.Gui.ViewModels
{
    public class RootViewModel
    {
        public RootViewModel(IIsopClient isopClient, Isop.Client.Models.Root root = null)
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
        private Isop.Client.IIsopClient isopClient;
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
            await Invoke(root, CurrentMethod.Method, CurrentMethod);
            return CurrentMethod;
        }

        private async Task<IReceiveResult> Invoke(Isop.Client.Models.Root root, Isop.Client.Models.Method method, IReceiveResult result)
        {
            try
            {
                result.Result = String.Empty;
                result.Error = null;
                result.ErrorMessage = String.Empty;
                using (var rstream = await isopClient.Invoke(root, method, r => r.Stream()))
                {
                    if (null != rstream.Stream)
                        using (var reader = new StreamReader(rstream.Stream, Encoding.UTF8))
                        {
                            while (true)
                            {
                                var line = await reader.ReadLineAsync();
                                if (line == null)
                                {
                                    break;
                                }
                                result.Result += line;
                            }
                        }

                    return result;
                }
            }
            catch (AggregateException aggEx)
            {
                if (aggEx.InnerExceptions.Count == 1 && aggEx.InnerExceptions.Any(e => e is RequestException))
                {
                    var requestException = (RequestException)aggEx.InnerExceptions.Single();
                    var errorObject = requestException.ErrorObject();
                    if (null != errorObject)
                    {
                        result.Error = errorObject;
                        result.ErrorMessage = errorObject.Message;
                        return result;
                    }
                }
                throw;
            }
            catch (RequestException ex)
            {
                var errorObject = ex.ErrorObject();
                if (null != errorObject)
                {
                    result.Error = errorObject;
                    result.ErrorMessage = errorObject.Message;
                    return result;
                }
                throw;
            }
        }
    }
}