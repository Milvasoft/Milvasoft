namespace Milvasoft.Core.Exceptions;

/// <summary>
/// Test exception class.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="MilvaTestException"/> class  with a specified error message.
/// </remarks>
/// <param name="exceptionMessage"></param>
public class MilvaTestException(string exceptionMessage) : Exception(exceptionMessage)
{
}
