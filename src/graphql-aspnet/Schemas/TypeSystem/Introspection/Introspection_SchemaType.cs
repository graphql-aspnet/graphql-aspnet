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
    /// Represents the graph type named '__Schema'.
    /// </summary>
    [DebuggerDisplay("INTROSPECTION TYPE __Schema")]
    internal class Introspection_SchemaType : BaseIntrospectionObjectType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Introspection_SchemaType"/> class.
        /// </summary>
        public Introspection_SchemaType()
            : base(Constants.ReservedNames.SCHEMA_TYPE, nameof(Introspection_SchemaType))
        {
            // "__Schema" type definition
            // https://graphql.github.io/graphql-spec/October2021/#sec-Introspection
            // -------------------------------------------------------------------------
            this.AddField<IntrospectedSchema, string>(
                "description",
                $"{this.InternalName}.{nameof(IntrospectedSchema.Description)}",
                new GraphTypeExpression(Constants.ScalarNames.STRING, GraphTypeExpression.SingleItem),
                new IntrospectedRoutePath(SchemaItemPathCollections.Types, this.Name, "description"),
                (its) => its.Description.AsCompletedTask(),
                "A human-readable string describing this schema.");

            this.AddField<IntrospectedSchema, IEnumerable<IntrospectedType>>(
                "types",
                $"{this.InternalName}.{nameof(IntrospectedSchema.KnownTypes)}",
                new GraphTypeExpression(Constants.ReservedNames.TYPE_TYPE, GraphTypeExpression.RequiredListRequiredItem),
                new IntrospectedRoutePath(SchemaItemPathCollections.Types, this.Name, "types"),
                (its) => (its?.KnownTypes).Where(x => x.Publish).AsCompletedTask(),
                "A complete collection of graph types declared by this schema.");

            this.AddField<IntrospectedSchema, IntrospectedType>(
                "queryType",
                $"{this.InternalName}.{nameof(IntrospectedSchema.QueryType)}",
                new GraphTypeExpression(Constants.ReservedNames.TYPE_TYPE, MetaGraphTypes.IsNotNull),
                new IntrospectedRoutePath(SchemaItemPathCollections.Types, this.Name, "queryType"),
                (its) => its.QueryType.AsCompletedTask(),
                "The root query type of this schema.");

            this.AddField<IntrospectedSchema, IntrospectedType>(
                "mutationType",
                $"{this.InternalName}.{nameof(IntrospectedSchema.MutationType)}",
                new GraphTypeExpression(Constants.ReservedNames.TYPE_TYPE),
                new IntrospectedRoutePath(SchemaItemPathCollections.Types, this.Name, "mutationType"),
                (its) => its.MutationType.AsCompletedTask(),
                "The root mutation type of this schema.");

            this.AddField<IntrospectedSchema, IntrospectedType>(
                "subscriptionType",
                $"{this.InternalName}.{nameof(IntrospectedSchema.SubscriptionType)}",
                new GraphTypeExpression(Constants.ReservedNames.TYPE_TYPE),
                new IntrospectedRoutePath(SchemaItemPathCollections.Types, this.Name, "subscriptionType"),
                (its) => its.SubscriptionType.AsCompletedTask(),
                "The root subscription type of this schema. Will be null if this server does not support subscriptions.");

            this.AddField<IntrospectedSchema, IEnumerable<IntrospectedDirective>>(
                "directives",
                $"{this.InternalName}.{nameof(IntrospectedSchema.DeclaredDirectives)}",
                new GraphTypeExpression(Constants.ReservedNames.DIRECTIVE_TYPE, GraphTypeExpression.RequiredListRequiredItem),
                new IntrospectedRoutePath(SchemaItemPathCollections.Types, this.Name, "directives"),
                (its) => its.DeclaredDirectives.Where(x => x.Publish).AsCompletedTask(),
                "A complete collection of the directives supported by this schema.");
        }
    }
}