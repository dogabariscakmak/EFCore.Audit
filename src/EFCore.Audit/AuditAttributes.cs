using System;

namespace EFCore.Audit
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class AuditableAttribute : Attribute
    { }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class NotAuditableAttribute : Attribute
    { }
}
