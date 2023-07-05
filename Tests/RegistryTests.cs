using FluentAssertions;
using Xunit;

namespace MyIoc.Tests;

public class RegistryTests
{
    private readonly Registry _sut;

    public RegistryTests()
    {
        _sut = new Registry();
    }
    
    [Fact]
    public void Should_resolve_type_with_manual_factory()
    {
        _sut.Register(_ => new MyClass(5));
        
        var instance = _sut.Resolve<MyClass>();

        instance.Should().BeEquivalentTo(new MyClass(5));
    }
    
    [Fact]
    public void Should_throw_if_service_was_not_registered()
    {
        Assert.Throws<InvalidOperationException>(() => _sut.Resolve<MyClass>());    
    }
    
    [Fact]
    public void Should_resolve_type_with_automatic_factory()
    {
        _sut.Register<MyOtherClass>();
        
        var instance = _sut.Resolve<MyOtherClass>();

        instance.Should().NotBeNull();
    }
    
    [Fact]
    public void Should_resolve_type_with_dependencies()
    {
        _sut.Register(_ => new MyClass(10));
        _sut.Register<MyDependentClass>();
        
        var instance = _sut.Resolve<MyDependentClass>();

        instance.Should().BeEquivalentTo(new MyDependentClass(new MyClass(10)));
    }
    
    [Fact]
    public void Should_throw_if_trying_to_resolve_automatic_factory_with_primitive_constructor_arguments()
    {
        _sut.Register<MyClass>();

        Assert.Throws<InvalidOperationException>(() => _sut.Resolve<MyClass>());
    }

    [Fact]
    public void Should_register_interface_types()
    {
        _sut.Register<IMyOtherInterface, MyOtherClass>();

        var service = _sut.Resolve<IMyOtherInterface>();

        service.Should().BeOfType<MyOtherClass>();
    }

    [Fact]
    public void Should_throw_if_trying_to_resolve_service_without_a_constructor()
    {
        _sut.Register<MyConstructorLessClass>();
        
        Assert.Throws<InvalidOperationException>(() => _sut.Resolve<MyConstructorLessClass>());
    }

    [Fact]
    public void Should_resolve_generic_services()
    {
        _sut.Register(_ => new MyClass(10));
        _sut.Register(typeof(MyGenericService<>));
        
        var service = _sut.Resolve<MyGenericService<MyClass>>();
        
        service.Should().NotBeNull();
    }

    [Fact]
    public void Should_resolve_generic_interfaces()
    {
        _sut.Register<MyOtherClass>();
        _sut.Register(typeof(IMyGenericInterface<>), typeof(MyGenericService<>));

        var service = _sut.Resolve<IMyGenericInterface<MyOtherClass>>();
        
        service.Should().BeOfType<MyGenericService<MyOtherClass>>();
    }

    [Fact]
    public void Should_resolve_generic_type_with_specified_arguments()
    {
        _sut.Register(_ => new MyGenericService<MyClass>(new MyClass(10)));

        var service = _sut.Resolve<MyGenericService<MyClass>>();
        
        service.Should().NotBeNull();
    }

    [Fact]
    public void Should_favour_generic_type_with_specified_arguments()
    {
        _sut.Register<MyOtherClass>();
        _sut.Register(typeof(MyGenericService<>));
        _sut.Register(_ => new MyGenericService<MyOtherClass>(new MyOtherClass { Value = 55 }));

        var service = _sut.Resolve<MyGenericService<MyOtherClass>>();

        service.Value.Value.Should().Be(55);
    }
    
    [Fact]
    public void Can_use_registry_to_create_factory()
    {
        _sut.Register(_ => new MyClass(10));
        _sut.Register<MyDependentClass>(context => new MyDependentClass(context.Registry.Resolve<MyClass>()));

        var service = _sut.Resolve<MyDependentClass>();
        
        service.Should().NotBeNull();
    }

    [Fact]
    public void Should_throw_if_trying_to_resolve_service_with_unrelated_implementation()
    {
        _sut.Register(typeof(MyClass), typeof(MyOtherClass));

        Assert.Throws<InvalidOperationException>(() => _sut.Resolve<MyClass>());
    }

    [Fact]
    public void Should_throw_if_registering_the_same_service_key_twice()
    {
        _sut.Register<MyClass>();

        Assert.Throws<InvalidOperationException>(() => _sut.Register<MyClass>());
    }

    [Fact]
    public void Should_only_create_one_object_per_type_when_resolving_a_tree()
    {
        _sut.Register<MyClass>(_ => new MyClass(10));
        _sut.Register<MyDependentClass>();
        _sut.Register<MyOtherDependentClass>();
        
        var service = _sut.Resolve<MyOtherDependentClass>();
        
        service.MyDependentClass.MyClass.Should().BeSameAs(service.MyClass);
    }

    [Fact]
    public void Should_only_create_one_object_per_generic_type_when_resolving_a_tree()
    {
        _sut.Register<MyOtherClass>();
        _sut.Register(typeof(MyGenericService<>));
        _sut.Register<MyGenericConsumer>();
        
        var service = _sut.Resolve<MyGenericConsumer>();

        service.ServiceOne.Should().BeSameAs(service.ServiceTwo);
    }

    [Fact]
    public void Should_throw_if_specifying_an_interface_as_implementation()
    {
        Assert.Throws<InvalidOperationException>(() => _sut.Register<IMyOtherInterface>());
    }

    private class MyClass
    {
        public int Value { get; }

        public MyClass(int value)
        {
            Value = value;
        }
    }
    
    private interface IMyOtherInterface
    {
    }
    
    private class MyOtherClass : IMyOtherInterface
    {
        public int Value { get; init; }
    }

    private class MyDependentClass
    {
        public MyClass MyClass { get; }

        public MyDependentClass(MyClass myClass)
        {
            MyClass = myClass;
        }
    }

    private class MyOtherDependentClass
    {
        public MyDependentClass MyDependentClass { get; }
        public MyClass MyClass { get; }

        public MyOtherDependentClass(MyDependentClass myDependentClass, MyClass myClass)
        {
            MyDependentClass = myDependentClass;
            MyClass = myClass;
        }    
    }
    
    private class MyConstructorLessClass
    {
        private MyConstructorLessClass()
        {
            
        }
    }

    private interface IMyGenericInterface<T>
    {
    }
    
    private class MyGenericService<T> : IMyGenericInterface<T>
    {
        public T Value { get; }

        public MyGenericService(T value)
        {
            Value = value;
        }   
    }

    private record MyGenericConsumer(MyGenericService<MyOtherClass> ServiceOne, MyGenericService<MyOtherClass> ServiceTwo);
}