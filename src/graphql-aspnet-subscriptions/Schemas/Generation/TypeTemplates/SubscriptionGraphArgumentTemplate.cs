// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas.Generation.TypeTemplates
{
    using System.Reflection;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Interfaces.Internal;

    /// <summary>
    /// An input argument template that is capable of parsing additional information
    /// related to subscriptions.
    /// </summary>
    public class SubscriptionGraphArgumentTemplate : GraphArgumentTemplate
    {
        private readonly bool _requireSourceDeclaration;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionGraphArgumentTemplate" /> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="resourceSubscriptionSourceDeclaration">if set to <c>true</c> this field must be tagged with <see cref="SubscriptionSourceAttribute"/> to be marked
        /// as a source parameter.</param>
        public SubscriptionGraphArgumentTemplate(
            IGraphFieldTemplateBase parent,
            ParameterInfo parameter,
            bool resourceSubscriptionSourceDeclaration = false)
            : base(parent, parameter)
        {
            _requireSourceDeclaration = resourceSubscriptionSourceDeclaration;
        }

        /// <inheritdoc />
        protected override bool IsSourceDataArgument()
        {
            if (this.Parameter.HasAttribute<SubscriptionSourceAttribute>())
                return true;

            if (_requireSourceDeclaration)
                return false;

            return base.IsSourceDataArgument();
        }
    }
}