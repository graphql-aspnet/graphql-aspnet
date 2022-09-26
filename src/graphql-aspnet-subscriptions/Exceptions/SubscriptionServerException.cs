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
    /// A general exception thrown when something related to subscription
    /// server component was unhandled.
    /// </summary>
    public class SubscriptionServerException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionServerException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public SubscriptionServerException(string message)
            : base(message)
        {
        }
    }
}