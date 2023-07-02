using System.Reflection;
using FluentAssertions;
using Xunit;

namespace MyIoc.Tests;

public class AutoRegisteringTests
{
    private readonly Registry _registry = new();

    public class FromList : AutoRegisteringTests
    {
        [Fact]
        public void Should_register_all_given_types()
        {
            AutoRegister(new[] { typeof(ServiceOne), typeof(ServiceTwo) });

            Resolve<ServiceOne>().Should().NotBeNull();
            Resolve<ServiceTwo>().Should().NotBeNull();
        }

        [Fact]
        public void Should_register_implemented_interfaces_as_service_keys()
        {
            AutoRegister(new[] { typeof(ServiceOne) });

            Resolve<IServiceOne>().Should().BeOfType<ServiceOne>();
        }

        [Fact]
        public void Should_only_register_interfaces_fulfilling_the_specified_rule()
        {
            _registry.AutoRegister(new[] { typeof(ServiceOne) }, types => types.Interface.Name.Contains("Apa"));
        
            Assert.Throws<InvalidOperationException>(Resolve<IServiceOne>);    
        }
        
        private void AutoRegister(IEnumerable<Type> services)
        {
            _registry.AutoRegister(services, _ => true);
        }
    }

    public class FromAssembly : AutoRegisteringTests
    {
        [Fact]
        public void Should_register_types_from_assembly_fulfilling_the_specified_rule()
        {
            _registry.RegisterFromAssembly(
                typeof(ServiceOne).Assembly,
                rules => rules
                    .WithRule(t => t.Name == "ServiceOne")
                    .WithRule(t => typeof(AutoRegisteringTests).GetNestedTypes(BindingFlags.NonPublic).Contains(t)));

            Resolve<ServiceOne>().Should().NotBeNull();
            Assert.Throws<InvalidOperationException>(Resolve<ServiceTwo>);
        }
    }
    
    private TService Resolve<TService>() where TService : class
    {
        return _registry.Resolve<TService>();
    }


    private interface IServiceOne
    {
    
    }

    private class ServiceOne : IServiceOne
    {
    }

    private class ServiceTwo
    {
    }
}