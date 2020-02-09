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
    using GraphQL.AspNet.Internal.Interfaces;

    /// <summary>
    /// A set of parsed metadata, read from a declared <see cref="GraphController"/>, to properly
    /// populate graphQL fields from action methods, including subscription fields.
    /// </summary>
    [DebuggerDisplay("Controller: '{ObjectType.Name}', Route: '{Route.Path}'")]
    public class GraphSubscriptionControllerTemplate : GraphControllerTemplate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphSubscriptionControllerTemplate"/> class.
        /// </summary>
        /// <param name="controllerType">Type of the controller.</param>
        public GraphSubscriptionControllerTemplate(Type controllerType)
            : base(controllerType)
        {
        }

        /// <summary>
        /// When overridden in a child, allows the class to create custom template that inherit from <see cref="MethodGraphFieldTemplate" />
        /// to provide additional functionality or garuntee a certian type structure for all methods on this object template.
        /// </summary>
        /// <param name="methodInfo">The method information.</param>
        /// <returns>IGraphFieldTemplate.</returns>
        protected override IGraphTypeFieldTemplate CreateMethodFieldTemplate(MethodInfo methodInfo)
        {
            if (methodInfo == null)
                return null;

            if (methodInfo.HasAttribute<SubscriptionAttribute>() || methodInfo.HasAttribute<SubscriptionRootAttribute>())
                return new ControllerSubscriptionActionGraphFieldTemplate(this, methodInfo);

            return base.CreateMethodFieldTemplate(methodInfo);
        }
    }
}