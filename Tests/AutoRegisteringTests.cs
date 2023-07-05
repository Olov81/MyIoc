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
            RegisterFromAssembly(rules => rules.Include(t => t.Name == "ServiceOne"));

            Resolve<ServiceOne>().Should().NotBeNull();
            Assert.Throws<InvalidOperationException>(Resolve<ServiceTwo>);
        }

        [Fact]
        public void Should_exclude_interfaces_as_service_implementations()
        {
            RegisterFromAssembly(rules => rules.Include(x => x.Name == nameof(IServiceOne)));
            
            Assert.Throws<InvalidOperationException>(Resolve<IServiceOne>);
        }
        
        [Fact]
        public void Should_only_register_interfaces_fulfilling_the_specified_rule()
        {
            RegisterFromAssembly(rules => rules.RegisterInterface(x => x.Interface.Name != nameof(IServiceOne)));
            
            Assert.Throws<InvalidOperationException>(Resolve<IServiceOne>);
        }

        [Fact]
        public void Should_register_generic_interfaces()
        {
            RegisterFromAssembly(_ => _);
            var service = _registry.Resolve<ITemplateInterface<int>>();
            service.Should().BeOfType<TemplateImplementation<int>>();
        }
        
        private void RegisterFromAssembly(Func<RegistryRuleBuilder, RegistryRuleBuilder> configureRules)
        {
            _registry.RegisterFromAssembly(
                typeof(ServiceOne).Assembly,
                rules => configureRules(rules)
                    .Include(TypesInThisClass));
        }

        private static bool TypesInThisClass(Type type)
        {
            return typeof(AutoRegisteringTests).GetNestedTypes(BindingFlags.NonPublic).Contains(type);
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

    private interface ITemplateInterface<T>
    {
    }

    private class TemplateImplementation<T> : ITemplateInterface<T>
    {
    }
}