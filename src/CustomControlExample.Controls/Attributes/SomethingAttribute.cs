using System;

namespace CustomControlExample.Controls.Attributes
{
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public sealed class SomethingAttribute : Attribute
    {
    }
}
