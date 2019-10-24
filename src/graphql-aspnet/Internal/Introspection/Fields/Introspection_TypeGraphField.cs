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
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal.Introspection.Model;
    using GraphQL.AspNet.Internal.Introspection.Resolvers;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// Represents the meta-field called '__type' allowing for introspection of a single type in the schema. This field
    /// is automatically added to the root query type of any <see cref="ISchema"/> declared on this server and will not be published
    /// on any introspection queries.
    /// </summary>
    [DebuggerDisplay("Meta Field: " + Constants.ReservedNames.TYPE_FIELD)]
    internal class Introspection_TypeGraphField : MethodGraphField
    {
        private static readonly GraphFieldPath FIELD_PATH = new GraphFieldPath(GraphCollection.Query, Constants.ReservedNames.TYPE_FIELD);

        /// <summary>
        /// Initializes a new instance of the <see cref="Introspection_TypeGraphField"/> class.
        /// </summary>
        /// <param name="schema">The schema.</param>
        public Introspection_TypeGraphField(IntrospectedSchema schema)
            : base(
                Constants.ReservedNames.TYPE_FIELD,
                new GraphTypeExpression(Constants.ReservedNames.TYPE_TYPE),
                FIELD_PATH,
                FieldResolutionMode.PerSourceItem,
                resolver: new Schema_TypeFieldResolver(schema))
        {
            this.Arguments.AddArgument(
                "name",
                "name",
                new GraphTypeExpression(Constants.ScalarNames.STRING, MetaGraphTypes.IsNotNull),
                typeof(string));
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