// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Common.Extensions
{
    using System;
    using System.Linq;
    using Microsoft.AspNetCore.Authorization;

    /// <summary>
    /// Extension methods for working with the attributes supplied by the library.
    /// </summary>
    public static class AttributeExtensions
    {
        /// <summary>
        /// Inspects the given attribute's attributes to determine if its allowed to be applied more than once
        /// to a given entity.
        /// </summary>
        /// <remarks>
        /// Used primarily for runtime field configuration to ensure correct usage within the templating system.
        /// </remarks>
        /// <param name="attribute">The attribute to check.</param>
        /// <returns><c>true</c> if this instance can be applied to an entity multiple times the specified attribute; otherwise, <c>false</c>.</returns>
        public static bool CanBeAppliedMultipleTimes(this Attribute attribute)
        {
            var usage = attribute.GetType().GetCustomAttributes(typeof(AttributeUsageAttribute), true)
                .Cast<AttributeUsageAttribute>()
                .ToList();

            // the default is always false
            if (usage.Count == 0)
                return false;

            // take the first found instance in the inheritance stack
            return usage[0].AllowMultiple;
        }
    }
}