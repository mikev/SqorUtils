namespace Sqor.Utils.Injection
{
    public interface IBinding
    {
        IResolver Resolver { get; }
        IScope Scope { get; }
    }
}
