using System;

namespace Milvasoft.Testing.IntegrationTest.Attributes;

/// <summary>
/// Creates a client intance for integration tests.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class CreateClientAttribute : Attribute
{
    /// <summary>
    /// Constructor of <see cref="CreateClientAttribute"/>.
    /// </summary>
    /// <param name="fakeClientType"></param>
    /// <param name="getClientInstanceMethodName"></param>
    public CreateClientAttribute(Type fakeClientType, string getClientInstanceMethodName)
    {

    }
}
