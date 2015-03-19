namespace Sqor.Utils.Injection
{
    public interface IResolver
    {
        object Instantiate(Request request);
        void Activate(Request request, object o);
    }
}
