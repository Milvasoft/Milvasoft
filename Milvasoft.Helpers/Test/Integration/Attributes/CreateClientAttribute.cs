using System;

namespace Milvasoft.Helpers.Test.Integration.Attributes
{
    /// <summary>
    /// Creates a client intance for integration tests.
    /// </summary>
    public class CreateClientAttribute : Attribute
    {
        /// <summary>
        /// Constructor of <see cref="CreateClientAttribute"/>.
        /// </summary>
        /// <param name="fakeClientType"></param>
        /// <param name="getInstanceMethodName"></param>
        public CreateClientAttribute(Type fakeClientType, string getInstanceMethodName)
        {

        }
    }
}
