// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Defaults.TypeMakers
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Configuration.Formatting;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Internal.TypeTemplates;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Security;

    /// <summary>
    /// A maker capable of turning a <see cref="IGraphFieldBaseTemplate"/> into a usable field in an object graph
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

        /// <summary>
        /// Instantiates the graph field according to the data provided.
        /// </summary>
        /// <param name="formatter">The formatter.</param>
        /// <param name="template">The template.</param>
        /// <param name="securityGroups">The security groups.</param>
        /// <returns>MethodGraphField.</returns>
        protected override MethodGraphField InstantiateField(
            GraphNameFormatter formatter,
            IGraphTypeFieldTemplate template,
            List<FieldSecurityGroup> securityGroups)
        {
            var subTemplate = template as ControllerSubscriptionActionGraphFieldTemplate;
            if (subTemplate != null
                && subTemplate.FieldSource == GraphFieldTemplateSource.Action
                && subTemplate.Route.RootCollection == GraphCollection.Subscription)
            {
                return new SubscriptionMethodGraphField(
                    formatter.FormatFieldName(template.Name),
                    template.TypeExpression.CloneTo(formatter.FormatGraphTypeName(template.TypeExpression.TypeName)),
                    template.Route,
                    template.ObjectType,
                    template.DeclaredReturnType,
                    template.Mode,
                    template.CreateResolver(),
                    securityGroups,
                    subTemplate.EventName);
            }

            return base.InstantiateField(formatter, template, securityGroups);
        }
    }
}