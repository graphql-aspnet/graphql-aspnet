// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Internal.Introspection.Types
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// Represents the introspection enumeration '__TypeKind'.
    /// </summary>
    [DebuggerDisplay("INTROSPECTION TYPE __TypeKind")]
    internal class Introspection_TypeKindType : EnumGraphType
    {
        /// <summary>
        /// Gets the instance of this meta-type.
        /// </summary>
        /// <value>The instance.</value>
        public static Introspection_TypeKindType Instance { get; } = new Introspection_TypeKindType();

        /// <summary>
        /// Initializes static members of the <see cref="Introspection_TypeKindType"/> class.
        /// </summary>
        static Introspection_TypeKindType()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Introspection_TypeKindType"/> class.
        /// </summary>
        private Introspection_TypeKindType()
            : base(Constants.ReservedNames.TYPE_KIND_ENUM, typeof(TypeKind))
        {
            foreach (var value in Enum.GetValues(this.ObjectType))
            {
                var fi = this.ObjectType.GetField(value.ToString());
                if (fi.SingleAttributeOrDefault<GraphSkipAttribute>() != null)
                    continue;

                var description = fi.SingleAttributeOrDefault<DescriptionAttribute>()?.Description;
                this.AddOption(new GraphEnumOption(this.ObjectType, value.ToString(), description, true, false, null));
            }
        }
    }
}