// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Schema.RuntimeDefinitions
{
    /// <summary>
    /// A special marker interface to indicate a runtime resolved field is subscription enabled.
    /// </summary>
    public interface IGraphQLSubscriptionEnabledRuntimeResolvedFieldDefinition : IGraphQLRuntimeResolvedFieldDefinition
    {
        /// <summary>
        /// Gets or sets the name of the event that must be published from a mutation to invoke this
        /// subscription.
        /// </summary>
        /// <value>The name of the published event that identifies this subscription.</value>
        string EventName { get; set; }
    }
}