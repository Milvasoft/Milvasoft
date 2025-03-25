namespace Milvasoft.Core.Exceptions;

/// <summary>
/// Base exception class.
/// </summary>
public abstract class MilvaBaseException : Exception
{
    /// <summary>
    /// Gets or sets error code.
    /// </summary>
    public int ExceptionCode { get; set; }

    /// <summary>
    /// Gets or sets object of exception.
    /// Default value is null.
    /// </summary>
    public object[] ExceptionObject { get; set; } = [];

    /// <summary>
    /// Variable for exception middleware.
    /// Default value is true.
    /// </summary>
    public bool UseLocalizerKey { get; set; } = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="MilvaBaseException"/> class  with a specified error message.
    /// </summary>
    /// <param name="messageOrLocalizerKey"></param>
    protected MilvaBaseException(string messageOrLocalizerKey) : base(messageOrLocalizerKey)
        => ExceptionCode = ExceptionCode != 0 ? ExceptionCode : MilvaCodes.Exception0Base;

    /// <summary>
    /// Initializes a new instance of the <see cref="MilvaBaseException"/> class  with a specified error message.
    /// </summary>
    /// <param name="messageOrLocalizerKey"></param>
    /// <param name="exceptionObjects"></param>
    protected MilvaBaseException(string messageOrLocalizerKey, params object[] exceptionObjects) : base(messageOrLocalizerKey)
    {
        ExceptionObject = exceptionObjects;
        ExceptionCode = ExceptionCode != 0 ? ExceptionCode : MilvaCodes.Exception0Base;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MilvaBaseException"/> class  with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="messageOrLocalizerKey"></param>
    /// <param name="innerException"></param>
    protected MilvaBaseException(string messageOrLocalizerKey, Exception innerException) : base(messageOrLocalizerKey, innerException)
        => ExceptionCode = ExceptionCode != 0 ? ExceptionCode : MilvaCodes.Exception0Base;
}
