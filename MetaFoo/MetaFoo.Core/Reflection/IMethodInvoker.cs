using Optional;

namespace MetaFoo.Core.Reflection
{
    public interface IMethodInvoker
    {
        Option<object> Invoke(Option<string> methodName, params object[] arguments);
    }
}