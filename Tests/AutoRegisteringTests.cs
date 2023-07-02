using FluentAssertions;
using Xunit;

namespace MyIoc.Tests;

public class AutoRegisteringTests
{
    private readonly Registry _registry = new();

    [Fact]
    public void Should_register_all_given_types()
    {
        _registry.AutoRegister(new[] { typeof(ServiceOne), typeof(ServiceTwo) });

        _registry.Resolve<ServiceOne>().Should().NotBeNull();
        _registry.Resolve<ServiceTwo>().Should().NotBeNull();
    }

    [Fact]
    public void Should_register_implemented_interfaces_as_service_keys()
    {
        _registry.AutoRegister(new[] { typeof(ServiceOne) });

        _registry.Resolve<IServiceOne>().Should().BeOfType<ServiceOne>();
    }
}

public interface IServiceOne
{
    
}

public class ServiceOne : IServiceOne
{
}

public class ServiceTwo
{
}