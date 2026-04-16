// Polyfills required for C# 9/10 features when targeting netstandard2.1.
// These types are missing from the netstandard2.1 BCL but are needed by the compiler.
// They are internal, carry no runtime behaviour, and are markers only.
#if NETSTANDARD2_1
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }

    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    internal sealed class CallerArgumentExpressionAttribute : Attribute
    {
        public CallerArgumentExpressionAttribute(string parameterName)
        {
            ParameterName = parameterName;
        }
        public string ParameterName { get; }
    }
}
#endif
