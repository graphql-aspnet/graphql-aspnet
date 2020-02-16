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
    using System;
    using GraphQL.AspNet.Schemas.Structural;

    /// <summary>
    /// An interface defining an event on a schema that can be watched for by a listener.
    /// </summary>
    public interface IMonitoredSubscriptionEvent
    {
        /// <summary>
        /// Gets the type of the schema being monitored for.
        /// </summary>
        /// <value>The type of the schema.</value>
        Type SchemaType { get; }

        /// <summary>
        /// Gets an alternate short name of the event.
        /// </summary>
        /// <value>The name of the event.</value>
        string EventName { get; }

        /// <summary>
        /// Gets the fully qualified route defining the event.
        /// </summary>
        /// <value>The route.</value>
        GraphFieldPath Route { get; }
    }
}