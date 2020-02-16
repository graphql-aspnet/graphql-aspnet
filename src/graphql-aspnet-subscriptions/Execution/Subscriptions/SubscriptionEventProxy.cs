// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.Subscriptions
{
    using GraphQL.AspNet.Common;

    /// <summary>
    /// A intermediate key/value pair holding the name of an event to be raised and the data object
    /// to be raised. Data is held until formalized into a <see cref="SubscriptionEvent"/>
    /// when appropriate.
    /// </summary>
    public class SubscriptionEventProxy
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionEventProxy"/> class.
        /// </summary>
        /// <param name="eventName">Name of the event.</param>
        /// <param name="dataObject">The data object.</param>
        public SubscriptionEventProxy(string eventName, object dataObject)
        {
            this.EventName = Validation.ThrowIfNullEmptyOrReturn(eventName, nameof(eventName));
            this.DataObject = dataObject;
        }

        /// <summary>
        /// Gets the raw, uncast data object that was transmitted with the event.
        /// </summary>
        /// <value>The data object.</value>
        public object DataObject { get; }

        /// <summary>
        /// Gets the name of the event.
        /// </summary>
        /// <value>The name of the event.</value>
        public string EventName { get; }
    }
}