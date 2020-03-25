using System;

namespace Lykke.Service.Sessions.Attributes
{
    /// <summary>
    ///     Attribute to mark sensitive data.
    ///     This data should not be logged.
    ///     Attribute allowed only on value types.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Struct)]
    public class SensitiveDataAttribute : Attribute
    {
    }
}