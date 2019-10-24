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
    /// Represents the introspection enumeration '__DirectiveLocation'.
    /// </summary>
    [DebuggerDisplay("INTROSPECTION TYPE __DirectiveLocation")]
    internal class Introspection_DirectiveLocationType : EnumGraphType
    {
        /// <summary>
        /// Gets the instance of this meta-type.
        /// </summary>
        /// <value>The instance.</value>
        public static Introspection_DirectiveLocationType Instance { get; } = new Introspection_DirectiveLocationType();

        /// <summary>
        /// Initializes static members of the <see cref="Introspection_DirectiveLocationType"/> class.
        /// </summary>
        static Introspection_DirectiveLocationType()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Introspection_DirectiveLocationType"/> class.
        /// </summary>
        private Introspection_DirectiveLocationType()
            : base(Constants.ReservedNames.DIRECTIVE_LOCATION_ENUM, typeof(DirectiveLocation))
        {
            // parse the enum values for later injection
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