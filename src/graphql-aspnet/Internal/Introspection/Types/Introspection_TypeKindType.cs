﻿// *************************************************************
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
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// Represents the introspection enumeration '__TypeKind'.
    /// </summary>
    [DebuggerDisplay("INTROSPECTION TYPE __TypeKind")]
    internal class Introspection_TypeKindType : EnumGraphType, IInternalSchemaItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Introspection_TypeKindType"/> class.
        /// </summary>
        public Introspection_TypeKindType()
            : base(
                  Constants.ReservedNames.TYPE_KIND_ENUM,
                  typeof(TypeKind),
                  new GraphIntrospectionFieldPath(Constants.ReservedNames.TYPE_KIND_ENUM))
        {
            foreach (var value in Enum.GetValues(this.ObjectType))
            {
                var fi = this.ObjectType.GetField(value.ToString());
                if (fi.SingleAttributeOrDefault<GraphSkipAttribute>() != null)
                    continue;

                var name = value.ToString();
                var description = fi.SingleAttributeOrDefault<DescriptionAttribute>()?.Description;
                var option = new GraphEnumValue(
                    this,
                    name,
                    description,
                    this.Route.CreateChild(name),
                    value,
                    name,
                    false,
                    null);

                this.AddOption(option);
            }
        }
    }
}