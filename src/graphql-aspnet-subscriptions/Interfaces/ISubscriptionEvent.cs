// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces
{
    using GraphQL.AspNet.Schemas.Structural;

    /// <summary>
    /// A reference to an event, raised during runtime, that indicates some action has occured
    /// that subscribed clients would be interested in.
    /// </summary>
    public interface ISubscriptionEvent
    {
        /// <summary>
        /// Gets the data package that was supplied at the publisher when the event was raised.
        /// </summary>
        /// <value>The data.</value>
        object Data { get; }

        /// <summary>
        /// Gets the name of the event that was raised.
        /// </summary>
        /// <value>The name of the event.</value>
        string EventName { get; }

        /// <summary>
        /// Gets the route of the method where this event was raised.
        /// </summary>
        /// <value>The source route.</value>
        GraphFieldPath SourceRoute { get; }
    }
}