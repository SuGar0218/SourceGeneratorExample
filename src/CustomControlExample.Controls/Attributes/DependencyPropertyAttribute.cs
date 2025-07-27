using System;

namespace CustomControlExample.Controls.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
    public sealed class DependencyPropertyAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
    public sealed class DependencyPropertyAttribute<T> : Attribute
    {
        public T DefaultValue { get; set; }
    }
}
