using System;
using System.Threading.Tasks;

namespace Isop.Abstractions
{
    public abstract class InvokeResult
    {
        public class Empty : InvokeResult
        {
        }

        public class ControllerAction(object? result) : InvokeResult
        {
            public object? Result { get; } = result;
        }

        public class Argument(object? result) : InvokeResult
        {
            public object? Result { get; } = result;
        }

        public class AsyncControllerAction(Task<object?> task) : InvokeResult
        {
            public Task<object?> Task { get; } = task;
        }

        /// <summary>
        /// Map from <see cref="InvokeResult"/> to <see typecref="T"/>.
        /// </summary>
        public T Select<T>(
            Func<ControllerAction, T> controllerAction, 
            Func<AsyncControllerAction, T> asyncControllerAction,
            Func<Argument, T> argument, 
            Func<Empty,T> empty)
        {
            return this switch
            {
                ControllerAction pm => controllerAction(pm),
                AsyncControllerAction am => asyncControllerAction(am),
                Argument a => argument(a),
                Empty e => empty(e),
                _ => throw new Exception("Unimplemented switch case"),
            };
        }
    }
}