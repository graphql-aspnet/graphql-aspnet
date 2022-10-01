// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Exceptions
{
    using System;
    using GraphQL.AspNet.Interfaces.Subscriptions;

    /// <summary>
    /// An exception thrown by a subscription server when its asked to
    /// accept a new connection that it deems unauthenticated.
    /// </summary>
    public class UnauthenticatedClientConnectionException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnauthenticatedClientConnectionException"/> class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        public UnauthenticatedClientConnectionException(IClientConnection connection)
        {
            this.Connection = connection;
        }

        /// <summary>
        /// Gets the connection that was deemed unauthenticated.
        /// </summary>
        /// <value>The connection.</value>
        public IClientConnection Connection { get; }
    }
}