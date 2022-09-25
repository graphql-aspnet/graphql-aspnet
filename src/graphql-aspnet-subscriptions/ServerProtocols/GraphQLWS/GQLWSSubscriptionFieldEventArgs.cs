﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ServerProtocols.GraphQLWS
{
    using System;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Schema.TypeSystem;

    /// <summary>
    /// An set of arguments related to the set of events that publish fields involved with subscription
    /// management.
    /// </summary>
    public class GQLWSSubscriptionFieldEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GQLWSSubscriptionFieldEventArgs" /> class.
        /// </summary>
        /// <param name="graphField">The graph field.</param>
        public GQLWSSubscriptionFieldEventArgs(ISubscriptionGraphField graphField)
        {
            this.Field = Validation.ThrowIfNullOrReturn(graphField, nameof(graphField));
        }

        /// <summary>
        /// Gets the field path route in context for this event.
        /// </summary>
        /// <value>The route.</value>
        public ISubscriptionGraphField Field { get; }
    }
}