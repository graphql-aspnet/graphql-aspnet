// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Apollo.Exceptions
{
    using System;

    /// <summary>
    /// A general exception thrown when something related to the apollo subscription
    /// server component was unhandled.
    /// </summary>
    public class ApolloSubscriptionServerException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApolloSubscriptionServerException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public ApolloSubscriptionServerException(string message)
            : base(message)
        {
        }
    }
}