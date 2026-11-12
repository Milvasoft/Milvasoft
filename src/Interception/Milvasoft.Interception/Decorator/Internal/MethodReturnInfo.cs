using System.Collections.Concurrent;
using System.Reflection;

namespace Milvasoft.Interception.Decorator.Internal;

/// <summary>
/// How an intercepted method delivers its result.
/// </summary>
internal enum MethodReturnKind
{
    /// <summary>
    /// The method returns its result (or nothing) synchronously.
    /// </summary>
    Sync,

    /// <summary>
    /// The method returns <see cref="Task"/>.
    /// </summary>
    Task,

    /// <summary>
    /// The method returns <see cref="Task{TResult}"/>.
    /// </summary>
    TaskOfT,

    /// <summary>
    /// The method returns <see cref="ValueTask"/>.
    /// </summary>
    ValueTask,

    /// <summary>
    /// The method returns <see cref="ValueTask{TResult}"/>.
    /// </summary>
    ValueTaskOfT,
}

/// <summary>
/// Describes the return type of an intercepted method and knows how to await the target's result and how to build the value the proxy returns.
/// Async-ness is decided by the declared return type only (never by <c>AsyncStateMachineAttribute</c>), so methods that return a task without
/// the <c>async</c> keyword and <see cref="ValueTask"/> returning methods are intercepted until their work really completes.
/// </summary>
internal sealed class MethodReturnInfo
{
    private static readonly ConcurrentDictionary<Type, MethodReturnInfo> _cache = new();

    private static readonly MethodInfo _getTaskResultMethod = typeof(MethodReturnInfo).GetMethod(nameof(GetTaskResult), BindingFlags.NonPublic | BindingFlags.Static);
    private static readonly MethodInfo _awaitValueTaskMethod = typeof(MethodReturnInfo).GetMethod(nameof(AwaitValueTaskResultAsync), BindingFlags.NonPublic | BindingFlags.Static);
    private static readonly MethodInfo _wrapTaskMethod = typeof(MethodReturnInfo).GetMethod(nameof(WrapAsTask), BindingFlags.NonPublic | BindingFlags.Static);
    private static readonly MethodInfo _wrapValueTaskMethod = typeof(MethodReturnInfo).GetMethod(nameof(WrapAsValueTask), BindingFlags.NonPublic | BindingFlags.Static);

    private readonly Func<Task, object> _getTaskResult;
    private readonly Func<object, Task<object>> _awaitValueTask;
    private readonly Func<Task, Call, object> _wrapResult;
    private readonly object _defaultSyncValue;

    /// <summary>
    /// Return kind of the method.
    /// </summary>
    public MethodReturnKind Kind { get; }

    /// <summary>
    /// Type of the produced value. For <see cref="Task{TResult}"/> and <see cref="ValueTask{TResult}"/> it is TResult,
    /// for <see cref="Task"/>, <see cref="ValueTask"/> and void methods it is <see cref="void"/>, otherwise the declared return type.
    /// </summary>
    public Type ResultType { get; }

    /// <summary>
    /// Whether the method is awaitable (<see cref="Task"/>, <see cref="Task{TResult}"/>, <see cref="ValueTask"/> or <see cref="ValueTask{TResult}"/>).
    /// </summary>
    public bool IsAsync => Kind != MethodReturnKind.Sync;

    private MethodReturnInfo(Type returnType)
    {
        if (returnType == typeof(Task))
        {
            Kind = MethodReturnKind.Task;
            ResultType = typeof(void);
        }
        else if (returnType == typeof(ValueTask))
        {
            Kind = MethodReturnKind.ValueTask;
            ResultType = typeof(void);
        }
        else if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            Kind = MethodReturnKind.TaskOfT;
            ResultType = returnType.GenericTypeArguments[0];
            _getTaskResult = _getTaskResultMethod.MakeGenericMethod(ResultType).CreateDelegate<Func<Task, object>>();
            _wrapResult = _wrapTaskMethod.MakeGenericMethod(ResultType).CreateDelegate<Func<Task, Call, object>>();
        }
        else if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(ValueTask<>))
        {
            Kind = MethodReturnKind.ValueTaskOfT;
            ResultType = returnType.GenericTypeArguments[0];
            _awaitValueTask = _awaitValueTaskMethod.MakeGenericMethod(ResultType).CreateDelegate<Func<object, Task<object>>>();
            _wrapResult = _wrapValueTaskMethod.MakeGenericMethod(ResultType).CreateDelegate<Func<Task, Call, object>>();
        }
        else
        {
            Kind = MethodReturnKind.Sync;
            ResultType = returnType;
            _defaultSyncValue = returnType.IsValueType && returnType != typeof(void) ? Activator.CreateInstance(returnType) : null;
        }
    }

    /// <summary>
    /// Gets the (cached) return information of <paramref name="method"/>.
    /// </summary>
    /// <param name="method"></param>
    /// <returns></returns>
    public static MethodReturnInfo For(MethodInfo method) => _cache.GetOrAdd(method.ReturnType, static type => new MethodReturnInfo(type));

    /// <summary>
    /// Awaits the value returned by the target method (if it is awaitable) and returns the produced result.
    /// Exceptions of the target's work are propagated to the caller.
    /// </summary>
    /// <param name="targetReturnValue">Value returned by the target method.</param>
    /// <returns>The unwrapped result of the target.</returns>
    public async Task<object> AwaitTargetResultAsync(object targetReturnValue)
    {
        switch (Kind)
        {
            case MethodReturnKind.Task:
                await EnsureNotNull<Task>(targetReturnValue).ConfigureAwait(false);
                return null;
            case MethodReturnKind.TaskOfT:
                var task = EnsureNotNull<Task>(targetReturnValue);
                await task.ConfigureAwait(false);
                return _getTaskResult(task);
            case MethodReturnKind.ValueTask:
                await ((ValueTask)targetReturnValue).ConfigureAwait(false);
                return null;
            case MethodReturnKind.ValueTaskOfT:
                return await _awaitValueTask(targetReturnValue).ConfigureAwait(false);
            default:
                return targetReturnValue;
        }
    }

    /// <summary>
    /// Builds the value that the proxy returns to the caller from the interception pipeline.
    /// For awaitable methods the returned task completes when the whole pipeline (including the target's work) completes.
    /// For synchronous methods the pipeline is awaited before returning.
    /// </summary>
    /// <param name="pipeline">The running interception pipeline.</param>
    /// <param name="call">The call whose <see cref="Call.ReturnValue"/> contains the result.</param>
    /// <returns></returns>
    public object CreateProxyReturnValue(Task pipeline, Call call)
    {
        switch (Kind)
        {
            case MethodReturnKind.Task:
                return pipeline;
            case MethodReturnKind.ValueTask:
                return new ValueTask(pipeline);
            case MethodReturnKind.TaskOfT:
            case MethodReturnKind.ValueTaskOfT:
                return _wrapResult(pipeline, call);
            default:
                // Synchronous methods cannot be awaited by the caller. The pipeline runs synchronously as long as the interceptors complete
                // synchronously; otherwise this blocks until they complete. GetResult rethrows the original exception (no AggregateException).
                pipeline.GetAwaiter().GetResult();

                return call.ReturnValue ?? _defaultSyncValue;
        }
    }

    private static T EnsureNotNull<T>(object value) where T : class
        => value as T ?? throw new InvalidOperationException($"The intercepted method returned null instead of a {typeof(T).Name}.");

    private static object GetTaskResult<T>(Task task) => ((Task<T>)task).Result;

    private static async Task<object> AwaitValueTaskResultAsync<T>(object valueTask) => await ((ValueTask<T>)valueTask).ConfigureAwait(false);

    private static object WrapAsTask<T>(Task pipeline, Call call) => GetResultAsync<T>(pipeline, call);

    private static object WrapAsValueTask<T>(Task pipeline, Call call) => new ValueTask<T>(GetResultAsync<T>(pipeline, call));

    private static async Task<T> GetResultAsync<T>(Task pipeline, Call call)
    {
        await pipeline.ConfigureAwait(false);

        return call.ReturnValue switch
        {
            null => default,
            T result => result,
            Task<T> resultTask => await resultTask.ConfigureAwait(false),
            _ => throw new InvalidOperationException($"Expected a return value of type '{typeof(T)}' but an interceptor set '{call.ReturnValue.GetType()}'.")
        };
    }
}
