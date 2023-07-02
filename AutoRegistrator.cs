using System.Reflection;

namespace MyIoc;

public static class RegistryExtensions
{
    public static void RegisterFromAssembly(
        this Registry registry,
        Assembly assembly,
        Func<RuleBuilder<Type>, RuleBuilder<Type>> configureRules)
    {
        var rule = configureRules(new RuleBuilder<Type>()).Build();
        
        registry.AutoRegister(assembly.GetTypes().Where(rule), _ => true);
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
                registry.Register(@interface, service);
            }
        }
    }
}