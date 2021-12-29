// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Internal.TypeTemplates
{
    using System.Reflection;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Internal.Interfaces;

    /// <summary>
    /// An input argument template that is capable of parsing additional information
    /// related to subscriptions.
    /// </summary>
    public class GraphSubscriptionInputArgumentTemplate : GraphInputArgumentTemplate
    {
        private readonly bool _requireSourceDeclaration;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphSubscriptionInputArgumentTemplate" /> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="resourceSubscriptionSourceDeclaration">if set to <c>true</c> this field must be tagged with <see cref="SubscriptionSourceAttribute"/> to be marked
        /// as a source parameter.</param>
        public GraphSubscriptionInputArgumentTemplate(
            IGraphFieldBaseTemplate parent,
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