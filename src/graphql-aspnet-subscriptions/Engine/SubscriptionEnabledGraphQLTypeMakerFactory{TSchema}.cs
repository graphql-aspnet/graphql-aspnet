// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Engine
{
    using System;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Engine.TypeMakers;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Internal.TypeTemplates;
    using GraphQL.AspNet.Schemas.Generation;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// An upgraded "type maker" factory that adds low level subscription field support
    /// to the type system.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this maker factory is registered to handle.</typeparam>
    public class SubscriptionEnabledGraphQLTypeMakerFactory<TSchema> : DefaultGraphQLTypeMakerFactory<TSchema>
        where TSchema : class, ISchema
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionEnabledGraphQLTypeMakerFactory{TSchema}" /> class.
        /// </summary>
        public SubscriptionEnabledGraphQLTypeMakerFactory()
        {
        }

        /// <inheritdoc />
        public override IGraphTypeTemplate MakeTemplate(Type objectType, TypeKind? kind = null)
        {
            if (Validation.IsCastable<GraphController>(objectType))
            {
                var template = new SubscriptionGraphControllerTemplate(objectType);
                template.Parse();
                template.ValidateOrThrow();
                return template;
            }

            return base.MakeTemplate(objectType, kind);
        }

        /// <inheritdoc />
        public override IGraphFieldMaker CreateFieldMaker()
        {
            return new SubscriptionEnabledGraphFieldMaker(this.Schema, this);
        }
    }
}