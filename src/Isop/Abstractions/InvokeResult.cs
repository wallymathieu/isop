#pragma warning disable CA1034 // Do not nest type
using System;
using System.Threading.Tasks;

namespace Isop.Abstractions;

///<summary>The result from invoking arguments (executing argument actions associated with arguments)</summary>
public abstract class InvokeResult
{
    private InvokeResult()
    {
    }

    /// <summary>
    /// An empty result means that for some reason we do not have any result from invocation.
    /// </summary>
    public class Empty : InvokeResult
    {
    }

    /// <summary>
    /// The result value result returned from when invoking a controller action. This result
    /// will <b>not</b> be a <see cref="Task" /> but some other type.
    /// </summary>
    public class ControllerAction(object? result) : InvokeResult
    {
        public object? Result { get; } = result;
    }

    /// <summary>
    /// The result of calling the action of an argument
    /// that might not be associated with a controller action.
    /// </summary>
    public class Argument(object? result) : InvokeResult
    {
        public object? Result { get; } = result;
    }
    /// <summary>
    /// The result <see cref="Task"/> result returned from when invoking a controller action.
    /// </summary>
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
        Func<Empty, T> empty)
    {
        if (controllerAction is null) throw new ArgumentNullException(nameof(controllerAction));
        if (asyncControllerAction is null) throw new ArgumentNullException(nameof(asyncControllerAction));
        if (argument is null) throw new ArgumentNullException(nameof(argument));
        if (empty is null) throw new ArgumentNullException(nameof(empty));
        return this switch
        {
            ControllerAction pm => controllerAction(pm),
            AsyncControllerAction am => asyncControllerAction(am),
            Argument a => argument(a),
            Empty e => empty(e),
            _ => throw new ArgumentException("Unimplemented switch case"),
        };
    }
}
