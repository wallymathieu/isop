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
            ignoreChanges = new SingleScopeOnly();
            foreach (var param in GlobalParameters)
            {
                param.PropertyChanged += globalValueChanged;
            }
        }
        public IEnumerable<Param> GlobalParameters { get; private set; }
        public IEnumerable<Controller> Controllers { get; private set; }
        private SingleScopeOnly ignoreChanges;
        private Method _currentMethod;
        public Method CurrentMethod
        {
            get { return _currentMethod; }
            set
            {
                if (null != _currentMethod)
                {
                    foreach (var param in _currentMethod.Parameters)
                    {
                        param.PropertyChanged -= methodValueChanged;
                    }
                }
                _currentMethod = value;
                if (null != _currentMethod)
                {
                    foreach (var param in GlobalParameters)
                    {
                        CurrentMethod.Parameters.SetParamValueOnMatching(param);
                    }
                    foreach (var param in _currentMethod.Parameters)
                    {
                        param.PropertyChanged += methodValueChanged;
                    }
                }
            }
        }

        private void globalValueChanged(object sender, PropertyChangedEventArgs args)
        {
            ignoreChanges.Try(() =>
            {
                if (null != CurrentMethod)
                {
                    CurrentMethod.Parameters.SetParamValueOnMatching(((Param)sender));
                }
            });
        }
        private void methodValueChanged(object sender, PropertyChangedEventArgs args)
        {
            ignoreChanges.Try(() =>
            {
                GlobalParameters.SetParamValueOnMatching(((Param)sender));
            });
        }
    }
}