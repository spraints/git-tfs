using System;

namespace Sep.Git.Tfs.Util
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public sealed class AfterRunAttribute : Attribute
    {
        public AfterRunAttribute(Type filterClass)
        {
            FilterClass = filterClass;
        }

        public Type FilterClass { get; set; }
    }
}
