namespace MyIoc;

public class Registry
{
    private readonly Dictionary<Type, Func<object>> _factories = new();

    public void Register<TService, TImplementation>() where TService : class where TImplementation : class, TService
    {
        Register<TService>(CreateAutoFactory<TImplementation>());
    }
    
    public void Register<TService>() where TService : class
    {
        Register(CreateAutoFactory<TService>());
    }
    
    public void Register<TService>(Func<TService> factory) where TService : class
    {
        _factories.Add(typeof(TService), factory);
    }
    
    public TService Resolve<TService>() where TService : class
    {
        return (Resolve(typeof(TService)) as TService)!;
    }
    
    private object Resolve(Type type)
    {
        if (!_factories.ContainsKey(type))
        {
            throw new InvalidOperationException($"Service {type.Name} was not registered");
        }
        
        return _factories[type]();
    }
    
    private Func<TImplementation> CreateAutoFactory<TImplementation>() where TImplementation : class
    {
        var type = typeof(TImplementation);
        
        var constructor = type.GetConstructors().SingleOrDefault();

        if (constructor is null)
        {
            throw new InvalidOperationException($"Type {type.Name} must have one and only one public constructor");
        }
        
        return () =>
        {        
            var parameters = constructor
                .GetParameters()
                .Select(p => Resolve(p.ParameterType))
                .ToArray();
            
            return (constructor.Invoke(parameters) as TImplementation)!;
        };
    }
}
