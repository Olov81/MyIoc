namespace MyIoc;

public class RuleBuilder<TInput>
{
    private readonly List<Func<TInput, bool>> _predicates = new();
    
    public RuleBuilder<TInput> WithRule(Func<TInput, bool> predicate)
    {
        _predicates.Add(predicate);
        return this;
    }
    
    public Func<TInput, bool> Build()
    {
        return input => _predicates.All(x => x(input));
    }
}