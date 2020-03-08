// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Apollo
{
    using System;
    using GraphQL.AspNet.Interfaces.Schema.TypeSystem;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// Event arguments containing the details of the event change.
    /// </summary>
    public class ApolloTrackedFieldArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApolloTrackedFieldArgs" /> class.
        /// </summary>
        /// <param name="field">The field.</param>
        public ApolloTrackedFieldArgs(ISubscriptionGraphField field)
        {
            this.Field = field;
        }

        /// <summary>
        /// Gets the subscription field that was newly subscribed to.
        /// </summary>
        /// <value>The name of the event.</value>
        public ISubscriptionGraphField Field { get; }
    }
}