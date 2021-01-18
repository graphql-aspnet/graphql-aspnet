// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Interfaces.Execution
{
    /// <summary>
    /// An extended field invocation context with additional properties related to subscriptions.
    /// </summary>
    public interface IGraphSubscriptionFieldInvocationContext : IGraphFieldInvocationContext
    {
        /// <summary>
        /// Gets the name of the event that maps to this field.
        /// </summary>
        /// <value>The name of the event.</value>
        string EventName { get; }
    }
}