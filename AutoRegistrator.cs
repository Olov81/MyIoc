namespace MyIoc;

public static class RegistryExtensions
{
    public static void AutoRegister(this Registry registry, IEnumerable<Type> services)
    {
        foreach (var service in services)
        {
            registry.Register(service);

            foreach (var @interface in service.GetInterfaces())
            {
                registry.Register(@interface, service);
            }
        }
    }
}