using System.Runtime.ExceptionServices;

namespace Milvasoft.Interception.Decorator.Internal;

internal static class ExceptionExtensions
{
    /// <summary>
    /// Captures <paramref name="exception"/> with <see cref="ExceptionDispatchInfo "/> and throw the <paramref name="exception"/>. 
    /// 
    /// <br/>
    /// 
    /// <remarks>
    /// ExceptionDispatchInfo is used to preserve the stack trace after an Exception is thrown, allowing you to catch that exception, 
    /// not throwing it immediately (as part of a catch), and to raise such exception on a later point in the future.
    /// </remarks>
    /// </summary>
    /// <param name="exception"></param>
    public static void Rethrow(this Exception exception)
        => ExceptionDispatchInfo.Capture(exception).Throw();
}
