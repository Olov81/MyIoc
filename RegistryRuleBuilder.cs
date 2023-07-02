namespace MyIoc;

public class RegistryRuleBuilder
{
    private readonly RuleBuilder<Type> _includeTypeRuleBuilder = new();
    private readonly RuleBuilder<(Type Interface, Type Implementation)> _registerInterfaceRuleBuilder = new();
    
    public RegistryRuleBuilder Include(Func<Type, bool> predicate)
    {
        _includeTypeRuleBuilder.WithRule(predicate);
        return this;
    }
    
    public RegistryRuleBuilder RegisterInterface(Func<(Type Interface, Type Implementation), bool> predicate)
    {
        _registerInterfaceRuleBuilder.WithRule(predicate);
        return this;
    }
    
    public Func<Type, bool> IncludeTypeRule => _includeTypeRuleBuilder.Aggregate();
    public Func<(Type Interface, Type Implementation), bool> RegisterInterfaceRule => _registerInterfaceRuleBuilder.Aggregate();
}