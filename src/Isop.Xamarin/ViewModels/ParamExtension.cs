using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;

namespace Isop.Xamarin
{
    public static class ViewModelExtension
    {
        public static void SetParamValueOnMatching(this IEnumerable<ParamViewModel> that, ParamViewModel updatedParam)
        {
            foreach (var param in that.Where(p => p.Name.Equals(updatedParam.Name, StringComparison.OrdinalIgnoreCase)))
            {
                param.Value = updatedParam.Value;
            }
        }

        public static void DeRegisterPropertyChanged(this IEnumerable<ParamViewModel> that, PropertyChangedEventHandler handler)
        {
            foreach (var param in that)
            {
                param.PropertyChanged -= handler;
            }
        }
        public static void RegisterPropertyChanged(this IEnumerable<ParamViewModel> that, PropertyChangedEventHandler handler)
        {
            foreach (var param in that)
            {
                param.PropertyChanged += handler;
            }
        }

        public static void SendPropertyChanged(this PropertyChangedEventHandler that, object sender, string name)
        {
            if (that != null)
            {
                that.Invoke(sender, new PropertyChangedEventArgs(name));
            }
        }
    }
}