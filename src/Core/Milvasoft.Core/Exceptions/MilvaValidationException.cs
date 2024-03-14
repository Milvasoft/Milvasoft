namespace Milvasoft.Core.Exceptions;

/// <summary>
/// The exception that is thrown when attempt to add an entity with Id the repository contains an entity with the same Id.
/// </summary>
public class MilvaValidationException : MilvaBaseException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MilvaValidationException"/> class with a specified error message.
    /// </summary>
    public MilvaValidationException() : base($"{MilvaException.Validation}Exception")
        => ExceptionCode = (int)MilvaException.Validation;

    /// <summary>
    /// Initializes a new instance of the <see cref="MilvaValidationException"/> class with a specified error message.
    /// </summary>
    /// <param name="messageOrLocalizerKey"></param>
    public MilvaValidationException(string messageOrLocalizerKey) : base(messageOrLocalizerKey)
        => ExceptionCode = (int)MilvaException.Validation;

    /// <summary>
    /// Initializes a new instance of the <see cref="MilvaValidationException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="messageOrLocalizerKey"></param>
    /// <param name="innerException"></param>
    public MilvaValidationException(string messageOrLocalizerKey, Exception innerException) : base(messageOrLocalizerKey, innerException)
        => ExceptionCode = (int)MilvaException.Validation;
}
