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
    /// messaging protcol not supported by the target schema.
    /// </summary>
    public class UnsupportedClientProtocolException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnsupportedClientProtocolException"/> class.
        /// </summary>
        /// <param name="protocol">The protocol.</param>
        public UnsupportedClientProtocolException(string protocol)
        {
            this.Protocol = protocol;
        }

        /// <summary>
        /// Gets the protocol that was attempted but not supported.
        /// </summary>
        /// <value>The protocol.</value>
        public string Protocol { get; }
    }
}