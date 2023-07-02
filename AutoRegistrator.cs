namespace MyIoc;

public static class RegistryExtensions
{
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