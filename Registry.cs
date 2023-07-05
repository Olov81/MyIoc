namespace MyIoc;

public interface IRegistry
{
    TService Resolve<TService>() where TService : class;
}

public class Registry : IRegistry
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
    
    public void Register<TService>(Func<Context, TService> factory) where TService : class
    {
        Register(typeof(TService), factory);
    }
    
    public void Register(Type type)
    {
        Register(type, CreateAutoFactory(type));
    }

    public void Register(Type type, Type implementationType)
    {
        Register(type, CreateAutoFactory(implementationType));
    }

    public TService Resolve<TService>() where TService : class
    {
        var implementation = Resolve(typeof(TService));

        return implementation as TService ?? throw new InvalidOperationException(
            $"{implementation.GetType().Name} must be convertible to {typeof(TService).Name}");
    }

    private void Register(Type type, Func<Context, object> factory)
    {
        if (_factories.ContainsKey(type))
        {
            throw new InvalidOperationException($"Service {type.Name} already registered");
        }
        
        _factories.Add(type, factory);
    }

    private object Resolve(Type type)
    {
        var context = new Context(type, this);
        return Resolve(context);
    }

    private object Resolve(Context context)
    {
        var type = context.RequestedType;
        
        if (context.ServiceCache.TryGetValue(type, out var cachedService))
        {
            return cachedService;
        }
        
        var factory = GetFactory(type);
        
        var service = factory(context);
        
        context.ServiceCache.Add(type, service);

        return service;
    }
    
    private Func<Context, object> GetFactory(Type type)
    {
        if (_factories.TryGetValue(type, out var factory))
        {
            return factory;
        }

        return type.IsGenericType && _factories.TryGetValue(type.GetGenericTypeDefinition(), out factory)
            ? factory
            : throw new InvalidOperationException($"Service {type.Name} was not registered");
    }
    
    private Func<Context, TImplementation> CreateAutoFactory<TImplementation>() where TImplementation : class
    {
        var factory = CreateAutoFactory(typeof(TImplementation));
        return context => (factory(context) as TImplementation)!;
    }

    private Func<Context, object> CreateAutoFactory(Type type)
    {
        if (type.IsInterface)
        {
            throw new InvalidOperationException("Interfaces are not valid as service implementations");    
        }
        
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
                .Select(p => Resolve(context with { RequestedType = p.ParameterType }))
                .ToArray()
                .Pipe(constructor.Invoke);
        };
    }

    public record Context(Type RequestedType, IRegistry Registry)
    {
        public readonly Dictionary<Type, object> ServiceCache = new();
    }
}
