// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Internal
{
    using System.Collections.Generic;
    using GraphQL.AspNet.SubscriptionServer;

    /// <summary>
    /// A collection of settings used by the dispatch queue to determine when it
    /// should raise logging events relating to the continuous dispatching of events.
    /// </summary>
    public interface ISubscriptionClientDispatchQueueAlertSettings
    {
        /// <summary>
        /// Gets the configured set of alert thresholds that the dispatch
        /// queue should alert at.
        /// </summary>
        /// <value>The configured thresholds.</value>
        IReadOnlyList<SubscriptionEventAlertThreshold> AlertThresholds { get; }
    }
}