namespace Milvasoft.UnitTests.CoreTests.HelperTests.CommonTests.Fixtures;

public class IsAssignableToTypeModelFixtures
{
    public class ClassImplementation { }
    public class GenericClassImplementation<T> { }
    public interface IInterface { }
    public interface IInterfaceImplementsIInterface : IInterface { }
    public interface IGenericInterface<T> { }
    public interface IGenericInterfaceImplementsInterface<T> : IInterface { }

    public class ClassImplementationWithInterface : IInterface { }
    public class GenericClassImplementationWithGenericInterface<T> : IGenericInterface<T> { }
    public class GenericClassImplementationWithInterface<T> : IInterface { }
    public class ClassImplementsClassImplementation : ClassImplementation { }
    public class GenericClassImplementsClassImplementation<T> : ClassImplementation { }
    public class GenericClassImplementsGenericClassImplementation<T> : GenericClassImplementation<T> { }
    public class ClassImplementsClassImplementationWithInterface : ClassImplementationWithInterface { }
    public class GenericClassImplementsGenericClassImplementationWithTwoArgument<T, T2> : GenericClassImplementation<T> { }
}
