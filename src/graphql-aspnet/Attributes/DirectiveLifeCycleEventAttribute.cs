// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Attributes
{
    using System;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// An attribute to tag any given <see cref="DirectiveLocation"/> value with
    /// its coorisponding <see cref="DirectiveLifeCycleEvent"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    internal class DirectiveLifeCycleEventAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DirectiveLifeCycleEventAttribute" /> class.
        /// </summary>
        /// <param name="lifeCycleEvent">The life cycle event to target.</param>
        public DirectiveLifeCycleEventAttribute(DirectiveLifeCycleEvent lifeCycleEvent)
        {
            this.LifeCycleEvent = lifeCycleEvent;
        }

        /// <summary>
        /// Gets the target phase indicating by this attribute.
        /// </summary>
        /// <value>The phase.</value>
        public DirectiveLifeCycleEvent LifeCycleEvent { get; }
    }
}