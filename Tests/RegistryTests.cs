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
        _sut.Register(() => new MyClass(5));
        
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
        _sut.Register(() => new MyClass(10));
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
    public void Should_throw_if_trying_to_automatically_register_service_without_a_constructor()
    {
        Assert.Throws<InvalidOperationException>(() => _sut.Register<MyConstructorLessClass>());
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
    }

    private class MyDependentClass
    {
        public MyClass MyClass { get; }

        public MyDependentClass(MyClass myClass)
        {
            MyClass = myClass;
        }
    }
    
    private class MyConstructorLessClass
    {
        private MyConstructorLessClass()
        {
            
        }
    }
}