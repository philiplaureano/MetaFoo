using Optional;

namespace MetaFoo.Reflection
{
    public interface IMethodInvoker
    {
        Option<object> Invoke(string methodName, params object[] arguments);
    }
}