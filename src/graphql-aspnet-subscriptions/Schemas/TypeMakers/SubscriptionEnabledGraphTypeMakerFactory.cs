// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas.TypeMakers
{
    using System;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.Generation.TypeMakers;
    using GraphQL.AspNet.Schemas.Generation.TypeTemplates;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// An upgraded "type maker" factory that adds low level subscription field support
    /// to the type system.
    /// </summary>
    public class SubscriptionEnabledGraphTypeMakerFactory : GraphTypeMakerFactory
    {
        private readonly ISchema _schemaInstance;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionEnabledGraphTypeMakerFactory" /> class.
        /// </summary>
        /// <param name="schemaInstance">The schema instance to reference when making
        /// types.</param>
        public SubscriptionEnabledGraphTypeMakerFactory(ISchema schemaInstance)
            : base(schemaInstance)
        {
            _schemaInstance = Validation.ThrowIfNullOrReturn(schemaInstance, nameof(schemaInstance));
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
            return new SubscriptionEnabledGraphFieldMaker(_schemaInstance, this.CreateArgumentMaker());
        }
    }
}