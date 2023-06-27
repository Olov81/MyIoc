﻿namespace MyIoc;

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
    
    private void Register(Type type, Func<Context, object> factory)
    {
        _factories.Add(type, factory);
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
        
        return _factories[typeWithoutTypeArgs](new Context(type, this));
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

    public record Context(Type RequestedType, IRegistry Registry);
}
