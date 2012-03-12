using System.Collections.Generic;
using System;
using System.ComponentModel;

namespace Isop.WpfControls.ViewModels
{
    public class MethodTreeModel
    {
        public MethodTreeModel(IEnumerable<Param> globalParameters, IEnumerable<Controller> controllers)
        {
            GlobalParameters=globalParameters;
            Controllers=controllers;
            singleEventHandlerScope = new SingleScopeOnly();
            GlobalParameters.RegisterPropertyChanged(globalValueChanged);
        }
        public IEnumerable<Param> GlobalParameters { get; private set; }
        public IEnumerable<Controller> Controllers { get; private set; }
        /// <summary>
        /// ignore changes if within another scope
        /// </summary>
        private SingleScopeOnly singleEventHandlerScope;
        private Method _currentMethod;
        public Method CurrentMethod
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
                    CurrentMethod.Parameters.SetParamValueOnMatching(((Param)sender));
                }
            });
        }
        private void methodValueChanged(object sender, PropertyChangedEventArgs args)
        {
            singleEventHandlerScope.Try(() =>
            {
                GlobalParameters.SetParamValueOnMatching(((Param)sender));
            });
        }
    }
}