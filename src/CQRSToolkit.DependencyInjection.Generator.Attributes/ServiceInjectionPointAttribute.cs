using System;

namespace CQRSToolkit.DependencyInjection.Generator.Attributes
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class ServiceInjectionPointAttribute : Attribute { }
}
