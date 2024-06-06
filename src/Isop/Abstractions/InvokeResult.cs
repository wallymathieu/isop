using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Isop.Abstractions
{
    public abstract class InvokeResult
    {
        public class Empty : InvokeResult
        {
        }

        public class ControllerAction(object result) : InvokeResult
        {
            public object Result { get=>result; }
        }

        public class Argument(object result) : InvokeResult
        {
            public object Result { get=>result; }
        }

        public class AsyncControllerAction(Task<object> task) : InvokeResult
        {
            public Task<object> Task { get=>task; }
        }
        #if NET8_0_OR_GREATER
        public class AsyncEnumerableControllerAction(IAsyncEnumerable<object> enumerable) : InvokeResult
        {
            public IAsyncEnumerable<object> Enumerable { get=>enumerable; }
        }
        #endif

        /// <summary>
        /// Map from <see cref="InvokeResult"/> to <see typecref="T"/>.
        /// </summary>
        public T Select<T>(
            Func<ControllerAction, T> controllerAction, 
            Func<AsyncControllerAction, T> asyncControllerAction, 
            #if NET8_0_OR_GREATER
            Func<AsyncEnumerableControllerAction, T> asyncEnumerableControllerAction, 
            #endif
            Func<Argument, T> argument, 
            Func<Empty,T> empty)
        {
            return this switch
            {
                ControllerAction pm => controllerAction(pm),
                AsyncControllerAction am => asyncControllerAction(am),
                #if NET8_0_OR_GREATER
                AsyncEnumerableControllerAction am => asyncEnumerableControllerAction(am),
                #endif
                Argument a => argument(a),
                Empty e => empty(e),
                _ => throw new Exception("Unimplemented switch case"),
            };
        }
    }
}