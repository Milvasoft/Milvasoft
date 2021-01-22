using System;

namespace Milvasoft.Helpers.Models
{
    /// <summary>
    /// Standart communication model. 
    /// For example, it can be used when you have two APIs communicating in a project and these APIs often need the same key when communicating.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public class CommunicationModel<T, TKey> where TKey : IEquatable<TKey>
    {
        /// <summary>
        /// Model transmitted during communication
        /// </summary>
        public virtual T TransmittedModel { get; set; }

        /// <summary>
        /// Key that must always be transmitted during communication.
        /// </summary>
        public virtual TKey TransmittedId { get; set; }
    }
}
