using System;

namespace Milvasoft.Helpers.Exceptions
{
    /// <summary>
    /// The exception that is thrown when attempt to add an entity with Id the repository contains an entity with the same Id.
    /// </summary>
    public class MilvaUserFriendlyException : MilvaBaseException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MilvaUserFriendlyException"/> class with a specified error message.
        /// </summary>
        public MilvaUserFriendlyException() : base($"{MilvaException.Base}Exception")
            => ExceptionCode = (int)MilvaException.Base;

        /// <summary>
        /// Initializes a new instance of the <see cref="MilvaUserFriendlyException"/> class with a specified error message.
        /// </summary>
        /// <param name="messageOrLocalizerKey"></param>
        public MilvaUserFriendlyException(string messageOrLocalizerKey) : base(messageOrLocalizerKey)
            => ExceptionCode = (int)MilvaException.Base;

        /// <summary>
        /// Initializes a new instance of the <see cref="MilvaBaseException"/> class  with a specified error message.
        /// </summary>
        /// <param name="messageOrLocalizerKey"></param>
        /// <param name="useLocalizerKey"></param>
        public MilvaUserFriendlyException(string messageOrLocalizerKey, bool useLocalizerKey) : base(messageOrLocalizerKey)
        {
            ExceptionCode = ExceptionCode != 0 ? ExceptionCode : (int)MilvaException.Base;
            UseLocalizerKey = useLocalizerKey;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MilvaUserFriendlyException"/> class with a specified error message.
        /// </summary>
        /// <param name="messageOrLocalizerKey"></param>
        /// <param name="exceptionCode"></param>
        public MilvaUserFriendlyException(string messageOrLocalizerKey, MilvaException exceptionCode) : base(messageOrLocalizerKey)
            => ExceptionCode = (int)exceptionCode;

        /// <summary>
        /// Initializes a new instance of the <see cref="MilvaUserFriendlyException"/> class with a specified error message.
        /// </summary>
        /// <param name="messageOrLocalizerKey"></param>
        /// <param name="exceptionCode"></param>
        /// <param name="exceptionObjects"></param>
        public MilvaUserFriendlyException(string messageOrLocalizerKey, MilvaException exceptionCode, params object[] exceptionObjects) : base(messageOrLocalizerKey)
        {
            ExceptionCode = (int)exceptionCode;
            ExceptionObject = exceptionObjects;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MilvaUserFriendlyException"/> class with a specified error message.
        /// </summary>
        /// <param name="messageOrLocalizerKey"></param>
        /// <param name="exceptionCode"></param>
        /// <param name="useLocalizerKey"></param>
        public MilvaUserFriendlyException(string messageOrLocalizerKey, MilvaException exceptionCode, bool useLocalizerKey) : base(messageOrLocalizerKey)
        {
            ExceptionCode = (int)exceptionCode;
            UseLocalizerKey = useLocalizerKey;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MilvaUserFriendlyException"/> class with a specified error message.
        /// </summary>
        /// <param name="messageOrLocalizerKey"></param>
        /// <param name="exceptionCode"></param>
        public MilvaUserFriendlyException(string messageOrLocalizerKey, int exceptionCode) : base(messageOrLocalizerKey)
            => ExceptionCode = exceptionCode;

        /// <summary>
        /// Initializes a new instance of the <see cref="MilvaUserFriendlyException"/> class with a specified error message.
        /// </summary>
        /// <param name="exceptionCode"></param>
        public MilvaUserFriendlyException(MilvaException exceptionCode) : base($"{exceptionCode}Exception")
            => ExceptionCode = (int)exceptionCode;

        /// <summary>
        /// Initializes a new instance of the <see cref="MilvaUserFriendlyException"/> class with a specified error message.
        /// </summary>
        /// <param name="exceptionCode"></param>
        /// <param name="exceptionObjects"></param>
        public MilvaUserFriendlyException(MilvaException exceptionCode, params object[] exceptionObjects) : base($"{exceptionCode}Exception")
        {
            ExceptionCode = (int)exceptionCode;
            ExceptionObject = exceptionObjects;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MilvaBaseException"/> class  with a specified error message.
        /// </summary>
        /// <param name="messageOrLocalizerKey"></param>
        /// <param name="exceptionObjects"></param>
        public MilvaUserFriendlyException(string messageOrLocalizerKey, params object[] exceptionObjects) : base(messageOrLocalizerKey, exceptionObjects)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MilvaBaseException"/> class  with a specified error message.
        /// </summary>
        /// <param name="messageOrLocalizerKey"></param>
        /// <param name="useLocalizerKey"></param>
        /// <param name="exceptionObjects"></param>
        public MilvaUserFriendlyException(string messageOrLocalizerKey, bool useLocalizerKey, params object[] exceptionObjects) : base(messageOrLocalizerKey)
        {
            ExceptionObject = exceptionObjects;
            ExceptionCode = ExceptionCode != 0 ? ExceptionCode : (int)MilvaException.Base;
            UseLocalizerKey = useLocalizerKey;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MilvaUserFriendlyException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="messageOrLocalizerKey"></param>
        /// <param name="innerException"></param>
        public MilvaUserFriendlyException(string messageOrLocalizerKey, Exception innerException) : base(messageOrLocalizerKey, innerException)
            => ExceptionCode = (int)MilvaException.Base;

        /// <summary>
        /// Initializes a new instance of the <see cref="MilvaUserFriendlyException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="messageOrLocalizerKey"></param>
        /// <param name="exceptionCode"></param>
        /// <param name="innerException"></param>
        public MilvaUserFriendlyException(string messageOrLocalizerKey, int exceptionCode, Exception innerException) : base(messageOrLocalizerKey, innerException)
            => ExceptionCode = exceptionCode;
    }
}
