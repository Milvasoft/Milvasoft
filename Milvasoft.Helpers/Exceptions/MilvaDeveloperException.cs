using System;

namespace Milvasoft.Helpers.Exceptions
{
    /// <summary>
    /// The exception that is thrown when attempt to add an entity with Id the repository contains an entity with the same Id.
    /// </summary>
    public class MilvaDeveloperException : MilvaBaseException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MilvaDeveloperException"/> class with a specified error message.
        /// </summary>
        public MilvaDeveloperException() : base($"{MilvaException.Base}Exception")
            => ExceptionCode = (int)MilvaException.Base;

        /// <summary>
        /// Initializes a new instance of the <see cref="MilvaDeveloperException"/> class with a specified error message.
        /// </summary>
        /// <param name="messageOrLocalizerKey"></param>
        public MilvaDeveloperException(string messageOrLocalizerKey) : base(messageOrLocalizerKey)
            => ExceptionCode = (int)MilvaException.Base;

        /// <summary>
        /// Initializes a new instance of the <see cref="MilvaDeveloperException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="messageOrLocalizerKey"></param>
        /// <param name="innerException"></param>
        public MilvaDeveloperException(string messageOrLocalizerKey, Exception innerException) : base(messageOrLocalizerKey, innerException)
            => ExceptionCode = (int)MilvaException.Base;
    }
}
