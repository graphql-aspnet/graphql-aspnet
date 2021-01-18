// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Interfaces.Schema.TypeSystem
{
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// A <see cref="IGraphField"/> containing information specificly related to subscriptions.
    /// </summary>
    public interface ISubscriptionGraphField : IGraphField
    {
        /// <summary>
        /// Gets the name of the event assigned to this subscription field.
        /// </summary>
        /// <value>The name of the event.</value>
        string EventName { get; }
    }
}