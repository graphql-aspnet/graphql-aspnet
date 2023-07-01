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
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Internal.TypeTemplates;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Security;

    /// <summary>
    /// A maker capable of turning a <see cref="IGraphFieldTemplateBase"/> into a usable field in an object graph
    /// with additional routines for supporting subscription fields.
    /// </summary>
    public class SubscriptionEnabledGraphFieldMaker : GraphFieldMaker
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionEnabledGraphFieldMaker"/> class.
        /// </summary>
        /// <param name="schema">The schema.</param>
        public SubscriptionEnabledGraphFieldMaker(ISchema schema)
            : base(schema)
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

                return new SubscriptionMethodGraphField(
                    formatter.FormatFieldName(template.Name),
                    template.TypeExpression.CloneTo(formatter.FormatGraphTypeName(template.TypeExpression.TypeName)),
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