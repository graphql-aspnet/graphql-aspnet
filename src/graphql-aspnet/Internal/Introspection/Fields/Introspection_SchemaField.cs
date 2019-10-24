// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Internal.Introspection.Fields
{
    using System.Diagnostics;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal.Introspection.Model;
    using GraphQL.AspNet.Internal.Resolvers;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.Structural;

    /// <summary>
    /// Represents the meta-field called '__schema' allowing for introspection into any schema. This field
    /// is automatically added to the root query type of any <see cref="ISchema"/> declared on this server and will not be published
    /// on any introspection queries.
    /// </summary>
    [DebuggerDisplay("Field: {Name}")]
    internal class Introspection_SchemaField : MethodGraphField
    {
        private static readonly GraphFieldPath FIELD_PATH = new GraphFieldPath(GraphCollection.Query, Constants.ReservedNames.SCHEMA_FIELD);

        /// <summary>
        /// Initializes a new instance of the <see cref="Introspection_SchemaField"/> class.
        /// </summary>
        /// <param name="schema">The schema.</param>
        public Introspection_SchemaField(IntrospectedSchema schema)
            : base(
                Constants.ReservedNames.SCHEMA_FIELD,
                new GraphTypeExpression(Constants.ReservedNames.SCHEMA_TYPE),
                FIELD_PATH,
                resolver: new GraphDataValueResolver<object, IntrospectedSchema>((x) => schema.AsCompletedTask()))
        {
            this.IntrospectedSchema = Validation.ThrowIfNullOrReturn(schema, nameof(schema));
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IGraphField"/> is published during
        /// introspection requests. False will indicate this field should never be included in any type
        /// information requests.
        /// </summary>
        /// <value><c>true</c> if this field should be published; otherwise, <c>false</c>.</value>
        public override bool Publish => false;

        /// <summary>
        /// Gets the introspected schema attached to this field.
        /// </summary>
        /// <value>The introspected schema.</value>
        public IntrospectedSchema IntrospectedSchema { get; }
    }
}