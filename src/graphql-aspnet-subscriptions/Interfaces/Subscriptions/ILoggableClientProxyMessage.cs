// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Subscriptions
{
    /// <summary>
    /// A base foundational set of properties all messages should implement
    /// if they wish to participate in logging events.
    /// </summary>
    public interface ILoggableClientProxyMessage
    {
        /// <summary>
        /// Gets a value indicating the type of the message, if any.
        /// </summary>
        /// <value>The type.</value>
        string Type { get; }

        /// <summary>
        /// Gets the optional identifier of this message.
        /// </summary>
        /// <value>The identifier.</value>
        string Id { get; }
    }
}