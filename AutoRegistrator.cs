using System.Reflection;

namespace MyIoc;

public static class RegistryExtensions
{
    public static void RegisterFromAssembly(
        this Registry registry,
        Assembly assembly,
        Func<RegistryRuleBuilder, RegistryRuleBuilder> configureRules)
    {
        var rules = configureRules(new RegistryRuleBuilder());
        
        registry.AutoRegister(
            assembly.GetTypes()
                .Where(t => t.IsClass)
                .Where(rules.IncludeTypeRule),
            rules.RegisterInterfaceRule);
    }
    
    public static void AutoRegister(
        this Registry registry,
        IEnumerable<Type> services,
        Func<(Type Interface, Type Implementation), bool> shouldRegisterInterface)
    {
        foreach (var service in services)
        {
            registry.Register(service);

            foreach (var @interface in service.GetInterfaces().Where(x => shouldRegisterInterface((x, service))))
            {
                var key = @interface.IsGenericType  ? @interface.GetGenericTypeDefinition() : @interface;
                registry.Register(key, service);
            }
        }
    }
}