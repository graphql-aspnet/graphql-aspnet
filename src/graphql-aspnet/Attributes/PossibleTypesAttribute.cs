// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Attributes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Common.Extensions;

    /// <summary>
    /// When an graph field (method or property) returns an interface, use this attribute
    /// to inform graphql of all the possible types that may be returned under that interface to ensure
    /// the object schema can correctly resolve all the items returned. This attribute is optional, if your application
    /// is configured to auto-register your graph types or you register them via the schema builder then this is not necessary.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
    public class PossibleTypesAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PossibleTypesAttribute" /> class.
        /// </summary>
        /// <param name="firstPossibleType">The first possible type to include.</param>
        /// <param name="additionalPossibleTypes">Any additional possible types.</param>
        public PossibleTypesAttribute(Type firstPossibleType, params Type[] additionalPossibleTypes)
        {
            this.PossibleTypes = firstPossibleType.AsEnumerable().Concat(additionalPossibleTypes).Where(x => x != null).ToArray();
        }

        /// <summary>
        /// Gets the possible types this field may return under its declared interface.
        /// </summary>
        /// <value>The possible types.</value>
        public IReadOnlyList<Type> PossibleTypes { get; }
    }
}