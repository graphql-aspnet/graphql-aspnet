﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas.TypeSystem.Introspection.Fields
{
    using System;
    using System.Diagnostics;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Resolvers.Introspeection;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Schemas.TypeSystem.Introspection.Model;

    /// <summary>
    /// Represents the meta-field called '__type' allowing for introspection of a single type in the schema. This field
    /// is automatically added to the root query type of any <see cref="ISchema"/> declared on this server and will not be published
    /// on any introspection queries.
    /// </summary>
    [DebuggerDisplay("Meta Field: " + Constants.ReservedNames.TYPE_FIELD)]
    internal class Introspection_TypeGraphField : MethodGraphField
    {
        private static readonly ItemPath FIELD_PATH = new ItemPath(ItemPathRoots.Query, Constants.ReservedNames.TYPE_FIELD);
        private readonly IntrospectedSchema _schema;

        /// <summary>
        /// Initializes a new instance of the <see cref="Introspection_TypeGraphField"/> class.
        /// </summary>
        /// <param name="schema">The schema.</param>
        public Introspection_TypeGraphField(IntrospectedSchema schema)
            : base(
                Constants.ReservedNames.TYPE_FIELD,
                nameof(Introspection_TypeGraphField),
                new GraphTypeExpression(Constants.ReservedNames.TYPE_TYPE),
                FIELD_PATH,
                declaredReturnType: typeof(IntrospectedType),
                objectType: typeof(IntrospectedType),
                mode: FieldResolutionMode.PerSourceItem,
                resolver: new Schema_TypeGraphFieldResolver(schema))
        {
            _schema = Validation.ThrowIfNullOrReturn(schema, nameof(schema));
            this.Arguments.AddArgument(
                "name",
                "name",
                new GraphTypeExpression(Constants.ScalarNames.STRING, MetaGraphTypes.IsNotNull),
                typeof(string));
        }

        /// <inheritdoc />
        public override IGraphField Clone(ISchemaItem parent = null, string fieldName = null, GraphTypeExpression typeExpression = null)
        {
            if (fieldName != null)
                throw new NotSupportedException($"{nameof(Introspection_TypeGraphField)} does not support field name changes.");
            if (typeExpression != null)
                throw new NotSupportedException($"{nameof(Introspection_TypeGraphField)} does not support type expression changes.");

            var item = new Introspection_TypeGraphField(_schema);
            item.Parent = parent ?? this.Parent;
            item.Description = this.Description;

            return item;
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IGraphField"/> is published during
        /// introspection requests. False will indicate this field should never be included in any type
        /// information requests.
        /// </summary>
        /// <value><c>true</c> if this field should be published; otherwise, <c>false</c>.</value>
        public override bool Publish => false;
    }
}