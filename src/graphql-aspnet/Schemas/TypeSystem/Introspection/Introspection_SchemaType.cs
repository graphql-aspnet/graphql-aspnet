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
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Schemas.TypeSystem.Introspection.Model;

    /// <summary>
    /// Represents the meta-type "__schema".
    /// </summary>
    [DebuggerDisplay("INTROSPECTION TYPE __Schema")]
    internal class Introspection_SchemaType : BaseIntrospectionObjectType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Introspection_SchemaType"/> class.
        /// </summary>
        public Introspection_SchemaType()
            : base(Constants.ReservedNames.SCHEMA_TYPE)
        {
            // "__Schema" type definition
            // https://graphql.github.io/graphql-spec/October2021/#sec-Introspection
            // -------------------------------------------------------------------------
            this.GraphFieldCollection.AddField<IntrospectedSchema, string>(
                "description",
                new GraphTypeExpression(Constants.ScalarNames.STRING, GraphTypeExpression.SingleItem),
                new IntrospectedRoutePath(GraphCollection.Types, this.Name, "description"),
                (its) => its.Description.AsCompletedTask(),
                "A human-readable string describing this schema.");

            this.GraphFieldCollection.AddField<IntrospectedSchema, IEnumerable<IntrospectedType>>(
                "types",
                new GraphTypeExpression(Constants.ReservedNames.TYPE_TYPE, GraphTypeExpression.RequiredListRequiredItem),
                new IntrospectedRoutePath(GraphCollection.Types, this.Name, "types"),
                (its) => (its?.KnownTypes).Where(x => x.Publish).AsCompletedTask(),
                "A complete collection of graph types declared by this schema.");

            this.GraphFieldCollection.AddField<IntrospectedSchema, IntrospectedType>(
                "queryType",
                new GraphTypeExpression(Constants.ReservedNames.TYPE_TYPE, MetaGraphTypes.IsNotNull),
                new IntrospectedRoutePath(GraphCollection.Types, this.Name, "queryType"),
                (its) => its.QueryType.AsCompletedTask(),
                "The root query type of this schema.");

            this.GraphFieldCollection.AddField<IntrospectedSchema, IntrospectedType>(
                "mutationType",
                new GraphTypeExpression(Constants.ReservedNames.TYPE_TYPE),
                new IntrospectedRoutePath(GraphCollection.Types, this.Name, "mutationType"),
                (its) => its.MutationType.AsCompletedTask(),
                "The root mutation type of this schema.");

            this.GraphFieldCollection.AddField<IntrospectedSchema, IntrospectedType>(
                "subscriptionType",
                new GraphTypeExpression(Constants.ReservedNames.TYPE_TYPE),
                new IntrospectedRoutePath(GraphCollection.Types, this.Name, "subscriptionType"),
                (its) => its.SubscriptionType.AsCompletedTask(),
                "The root subscription type of this schema. Will be null if this server does not support subscriptions.");

            this.GraphFieldCollection.AddField<IntrospectedSchema, IEnumerable<IntrospectedDirective>>(
                "directives",
                new GraphTypeExpression(Constants.ReservedNames.DIRECTIVE_TYPE, GraphTypeExpression.RequiredListRequiredItem),
                new IntrospectedRoutePath(GraphCollection.Types, this.Name, "directives"),
                (its) => its.DeclaredDirectives.Where(x => x.Publish).AsCompletedTask(),
                "A complete collection of the directives supported by this schema.");
        }
    }
}