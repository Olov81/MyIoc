namespace MyIoc;

public class Registry
{
    private readonly Dictionary<Type, Func<Context, object>> _factories = new();

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
        Register(_ => factory());
    }
    
    private void Register<TService>(Func<Context, TService> factory) where TService : class
    {
        _factories.Add(typeof(TService), factory);
    }
    
    public void Register(Type type)
    {
        _factories.Add(type, CreateAutoFactory(type));
    }

    public TService Resolve<TService>() where TService : class
    {
        return (Resolve(typeof(TService)) as TService)!;
    }
    
    private object Resolve(Type type)
    {
        var typeWithoutTypeArgs = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
        
        if (!_factories.ContainsKey(typeWithoutTypeArgs))
        {
            throw new InvalidOperationException($"Service {type.Name} was not registered");
        }
        
        return _factories[typeWithoutTypeArgs](new Context(type));
    }
    
    private Func<Context, TImplementation> CreateAutoFactory<TImplementation>() where TImplementation : class
    {
        var factory = CreateAutoFactory(typeof(TImplementation));
        return context => (factory(context) as TImplementation)!;
    }
    
    private Func<Context, object> CreateAutoFactory(Type type)
    {
        return context =>
        {
            var genericType = type.IsGenericType 
                ? type.MakeGenericType(context.RequestedType.GetGenericArguments()) 
                : type;
            
            var constructor = genericType.GetConstructors().SingleOrDefault();

            if (constructor is null)
            {
                throw new InvalidOperationException($"Type {type.Name} must have one and only one public constructor");
            }

            return constructor
                .GetParameters()
                .Select(p => Resolve(p.ParameterType))
                .ToArray()
                .Pipe(constructor.Invoke);
        };
    }

    private record Context(Type RequestedType);
}
