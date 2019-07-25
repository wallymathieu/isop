using System;
using System.Threading.Tasks;

namespace Isop.Abstractions
{
    public abstract class InvokeResult
    {
        public class Empty : InvokeResult
        {
        }

        public class ControllerAction : InvokeResult
        {
            public object Result { get; }

            public ControllerAction(object result)
            {
                Result = result;
            }
        }

        public class Argument : InvokeResult
        {
            public object Result { get; }

            public Argument(object result)
            {
                Result = result;
            }
        }

        public class AsyncControllerAction : InvokeResult
        {
            public Task<object> Task { get; }

            public AsyncControllerAction(Task<object> task)
            {
                Task = task;
            }
        }
        
        /// <summary>
        /// Map from <see cref="InvokeResult"/> to <see cref="T"/>.
        /// </summary>
        public T Select<T>(
            Func<ControllerAction, T> controllerAction, 
            Func<AsyncControllerAction, T> asyncControllerAction, 
            Func<Argument, T> argument, 
            Func<Empty,T> empty)
        {
            switch (this)
            {
                case ControllerAction pm: return controllerAction(pm);
                case AsyncControllerAction am: return asyncControllerAction(am);
                case Argument a: return argument(a);
                case Empty e: return empty(e);
                default:
                    throw new Exception("Unimplemented switch case");
            }
        }
    }
}