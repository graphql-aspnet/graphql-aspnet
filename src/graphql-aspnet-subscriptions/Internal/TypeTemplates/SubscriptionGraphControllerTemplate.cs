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
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Schemas.Generation.TypeTemplates;

    /// <summary>
    /// A set of parsed metadata, read from a declared <see cref="GraphController"/>, to properly
    /// populate graphQL fields from action methods, including subscription fields.
    /// </summary>
    [DebuggerDisplay("Controller: '{ObjectType.Name}', Route: '{Route.Path}'")]
    public class SubscriptionGraphControllerTemplate : GraphControllerTemplate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionGraphControllerTemplate"/> class.
        /// </summary>
        /// <param name="controllerType">Type of the controller.</param>
        public SubscriptionGraphControllerTemplate(Type controllerType)
            : base(controllerType)
        {
        }

        /// <inheritdoc />
        protected override IGraphFieldTemplate CreateFieldTemplate(IFieldMemberInfoProvider member)
        {
            if (member?.MemberInfo == null || !(member.MemberInfo is MethodInfo))
                return null;

            if (member?.MemberInfo != null &&
                member.MemberInfo is MethodInfo methodInfo &&
                (member.AttributeProvider.HasAttribute<SubscriptionAttribute>() || member.AttributeProvider.HasAttribute<SubscriptionRootAttribute>()))
            {
                return new SubscriptionControllerActionGraphFieldTemplate(this, methodInfo, member.AttributeProvider);
            }

            return base.CreateFieldTemplate(member);
        }
    }
}