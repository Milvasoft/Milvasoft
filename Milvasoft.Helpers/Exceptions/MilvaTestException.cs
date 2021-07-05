using System;

namespace Milvasoft.Helpers.Exceptions
{
    /// <summary>
    /// An error that will be launched during the Test.
    /// </summary>
    public class MilvaTestException : MilvaBaseException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MilvaTestException"/> class with a specified error message.
        /// </summary>
        public MilvaTestException() : base($"{MilvaException.Base}Exception")
            => ExceptionCode = (int)MilvaException.Base;

        /// <summary>
        /// Initializes a new instance of the <see cref="MilvaTestException"/> class with a specified error message.
        /// </summary>
        /// <param name="messageOrLocalizerKey"></param>
        public MilvaTestException(string messageOrLocalizerKey) : base(messageOrLocalizerKey)
            => ExceptionCode = (int)MilvaException.Base;

        /// <summary>
        /// Initializes a new instance of the <see cref="MilvaBaseException"/> class  with a specified error message.
        /// </summary>
        /// <param name="messageOrLocalizerKey"></param>
        /// <param name="useLocalizerKey"></param>
        public MilvaTestException(string messageOrLocalizerKey, bool useLocalizerKey) : base(messageOrLocalizerKey)
        {
            ExceptionCode = ExceptionCode != 0 ? ExceptionCode : (int)MilvaException.Base;
            UseLocalizerKey = useLocalizerKey;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MilvaTestException"/> class with a specified error message.
        /// </summary>
        /// <param name="messageOrLocalizerKey"></param>
        /// <param name="exceptionCode"></param>
        public MilvaTestException(string messageOrLocalizerKey, MilvaException exceptionCode) : base(messageOrLocalizerKey)
            => ExceptionCode = (int)exceptionCode;

        /// <summary>
        /// Initializes a new instance of the <see cref="MilvaTestException"/> class with a specified error message.
        /// </summary>
        /// <param name="messageOrLocalizerKey"></param>
        /// <param name="exceptionCode"></param>
        /// <param name="exceptionObjects"></param>
        public MilvaTestException(string messageOrLocalizerKey, MilvaException exceptionCode, params object[] exceptionObjects) : base(messageOrLocalizerKey)
        {
            ExceptionCode = (int)exceptionCode;
            ExceptionObject = exceptionObjects;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MilvaTestException"/> class with a specified error message.
        /// </summary>
        /// <param name="messageOrLocalizerKey"></param>
        /// <param name="exceptionCode"></param>
        public MilvaTestException(string messageOrLocalizerKey, int exceptionCode) : base(messageOrLocalizerKey)
            => ExceptionCode = exceptionCode;

        /// <summary>
        /// Initializes a new instance of the <see cref="MilvaTestException"/> class with a specified error message.
        /// </summary>
        /// <param name="exceptionCode"></param>
        public MilvaTestException(MilvaException exceptionCode) : base($"{exceptionCode}Exception")
            => ExceptionCode = (int)exceptionCode;

        /// <summary>
        /// Initializes a new instance of the <see cref="MilvaTestException"/> class with a specified error message.
        /// </summary>
        /// <param name="exceptionCode"></param>
        /// <param name="exceptionObjects"></param>
        public MilvaTestException(MilvaException exceptionCode, params object[] exceptionObjects) : base($"{exceptionCode}Exception")
        {
            ExceptionCode = (int)exceptionCode;
            ExceptionObject = exceptionObjects;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MilvaBaseException"/> class  with a specified error message.
        /// </summary>
        /// <param name="messageOrLocalizerKey"></param>
        /// <param name="exceptionObjects"></param>
        public MilvaTestException(string messageOrLocalizerKey, params object[] exceptionObjects) : base(messageOrLocalizerKey, exceptionObjects)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MilvaBaseException"/> class  with a specified error message.
        /// </summary>
        /// <param name="messageOrLocalizerKey"></param>
        /// <param name="useLocalizerKey"></param>
        /// <param name="exceptionObjects"></param>
        public MilvaTestException(string messageOrLocalizerKey, bool useLocalizerKey, params object[] exceptionObjects) : base(messageOrLocalizerKey)
        {
            ExceptionObject = exceptionObjects;
            ExceptionCode = ExceptionCode != 0 ? ExceptionCode : (int)MilvaException.Base;
            UseLocalizerKey = useLocalizerKey;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MilvaTestException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="messageOrLocalizerKey"></param>
        /// <param name="innerException"></param>
        public MilvaTestException(string messageOrLocalizerKey, Exception innerException) : base(messageOrLocalizerKey, innerException)
            => ExceptionCode = (int)MilvaException.Base;

        /// <summary>
        /// Initializes a new instance of the <see cref="MilvaTestException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="messageOrLocalizerKey"></param>
        /// <param name="exceptionCode"></param>
        /// <param name="innerException"></param>
        public MilvaTestException(string messageOrLocalizerKey, int exceptionCode, Exception innerException) : base(messageOrLocalizerKey, innerException)
            => ExceptionCode = exceptionCode;
    }
}
