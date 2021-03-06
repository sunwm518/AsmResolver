using System.Collections.Generic;
using System.Linq;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents a member that can be referenced by a HasCustomAttribute coded index,
    /// </summary>
    public interface IHasCustomAttribute : IMetadataMember
    {
        /// <summary>
        /// Gets a collection of custom attributes assigned to this member.
        /// </summary>
        IList<CustomAttribute> CustomAttributes { get; }
    }

    /// <summary>
    /// Provides extensions for various metadata members. 
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// Indicates whether the specified member is compiler generated.
        /// </summary>
        /// <param name="attribute">The referenced member to check</param>
        /// <returns><c>true</c> if the member was generated by the compiler, otherwise <c>false</c></returns>
        public static bool IsCompilerGenerated(this IHasCustomAttribute attribute)
        {
            return attribute.CustomAttributes.Any(c =>
                c.Constructor.DeclaringType.IsTypeOf("System.Runtime.CompilerServices", "CompilerGeneratedAttribute"));
        }
    }
}