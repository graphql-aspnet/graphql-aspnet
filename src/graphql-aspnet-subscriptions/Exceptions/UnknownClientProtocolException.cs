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

    /// <summary>
    /// An exception thrown when a connected client attempts to communicate over a
    /// messaging protcol unknown to this server instance.
    /// </summary>
    public class UnknownClientProtocolException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnknownClientProtocolException"/> class.
        /// </summary>
        /// <param name="protocol">The protocol.</param>
        public UnknownClientProtocolException(string protocol)
        {
            this.Protocol = protocol;
        }

        /// <summary>
        /// Gets the protocol that is not known to this server instance.
        /// </summary>
        /// <value>The protocol.</value>
        public string Protocol { get; }
    }
}