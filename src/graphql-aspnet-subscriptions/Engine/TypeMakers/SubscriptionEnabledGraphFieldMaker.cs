// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Engine.TypeMakers
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Configuration.Formatting;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Internal.TypeTemplates;
    using GraphQL.AspNet.Schemas.Generation.TypeMakers;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Security;

    /// <summary>
    /// A maker capable of turning a <see cref="IGraphFieldTemplateBase"/> into a usable field in an object graph
    /// with additional routines for supporting subscription fields.
    /// </summary>
    public class SubscriptionEnabledGraphFieldMaker : GraphFieldMaker
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionEnabledGraphFieldMaker" /> class.
        /// </summary>
        /// <param name="schema">The schema instance to reference when creating fields.</param>
        /// <param name="makerFactory">The maker factory to create dependnet makers
        /// if and when necessary.</param>
        public SubscriptionEnabledGraphFieldMaker(ISchema schema, IGraphQLTypeMakerFactory makerFactory)
            : base(schema, makerFactory)
        {
        }

        /// <inheritdoc />
        protected override MethodGraphField InstantiateField(
            GraphNameFormatter formatter,
            IGraphFieldTemplate template,
            List<AppliedSecurityPolicyGroup> securityGroups)
        {
            var subTemplate = template as SubscriptionControllerActionGraphFieldTemplate;
            if (subTemplate != null
                && subTemplate.FieldSource == GraphFieldSource.Action
                && subTemplate.Route.RootCollection == SchemaItemCollections.Subscription)
            {
                var directives = template.CreateAppliedDirectives();

                var schemaTypeName = this.PrepareTypeName(template);
                var typeExpression = template.TypeExpression.CloneTo(schemaTypeName);

                return new SubscriptionMethodGraphField(
                    formatter.FormatFieldName(template.Name),
                    typeExpression,
                    template.Route,
                    template.InternalName,
                    template.ObjectType,
                    template.DeclaredReturnType,
                    template.Mode,
                    template.CreateResolver(),
                    securityGroups,
                    subTemplate.EventName,
                    directives);
            }

            return base.InstantiateField(formatter, template, securityGroups);
        }
    }
}