// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas.TypeSystem.Introspection
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// Represents the introspection enumeration '__DirectiveLocation'.
    /// </summary>
    [DebuggerDisplay("INTROSPECTION TYPE __DirectiveLocation")]
    internal class Introspection_DirectiveLocationType : EnumGraphType, IInternalSchemaItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Introspection_DirectiveLocationType"/> class.
        /// </summary>
        public Introspection_DirectiveLocationType()
            : base(
                  Constants.ReservedNames.DIRECTIVE_LOCATION_ENUM,
                  typeof(DirectiveLocation),
                  new GraphIntrospectionFieldPath(Constants.ReservedNames.DIRECTIVE_LOCATION_ENUM))
        {
            foreach (var value in Enum.GetValues(this.ObjectType))
            {
                var fi = this.ObjectType.GetField(value.ToString());
                if (fi.SingleAttributeOrDefault<GraphSkipAttribute>() != null)
                    continue;

                var name = value.ToString();
                var description = fi.SingleAttributeOrDefault<DescriptionAttribute>()?.Description;
                var option = new EnumValue(
                    this,
                    name,
                    description,
                    this.Route.CreateChild(name),
                    value,
                    name);

                this.AddOption(option);
            }
        }
    }
}