using System;

namespace SPAFilter.SPA.SC
{
    public abstract class SComponentBase
    {
        public virtual string Name { get; protected set; }

        public virtual string Description { get; protected set; }

        public virtual string ToXml() => string.Empty;

        public override string ToString() => Name;
    }

    /// <inheritdoc />
    /// <summary>
    /// Indicates that the value of marked element could never be <c>null</c>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.Delegate | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class NotNullAttribute : Attribute
    {
    }
}
