namespace Milvasoft.Core.Exceptions;

/// <summary>
/// Test exception class.
/// </summary>
public class MilvaTestException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MilvaTestException"/> class  with a specified error message.
    /// </summary>
    /// <param name="exceptionMessage"></param>
    public MilvaTestException(string exceptionMessage) : base(exceptionMessage)
    {
    }
}
