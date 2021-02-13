using Optional;

namespace MetaFoo.Reflection
{
    public interface IMethodInvoker
    {
        Option<object> Invoke(Option<string> methodName, params object[] arguments);
    }
}