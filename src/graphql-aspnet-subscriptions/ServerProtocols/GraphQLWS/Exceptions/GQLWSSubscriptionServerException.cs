// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ServerProtocols.GraphQLWS.Exceptions
{
    using System;

    /// <summary>
    /// A general exception thrown when something related to the graphql-ws subscription
    /// server component was unhandled.
    /// </summary>
    public class GQLWSSubscriptionServerException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GQLWSSubscriptionServerException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public GQLWSSubscriptionServerException(string message)
            : base(message)
        {
        }
    }
}