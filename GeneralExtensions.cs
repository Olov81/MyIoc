namespace MyIoc;

public static class GeneralExtensions
{
    public static TU Pipe<T, TU>(this T t, Func<T, TU> f) => f(t);
}