using System;

namespace Isop.Xamarin
{
    internal static class Extensions
    {
        public static T Tap<T>(this T obj, Action<T> tapaction)
        {
            tapaction(obj);
            return obj;
        }
    }
}

