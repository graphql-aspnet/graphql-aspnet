// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ServerExtensions.MultipartRequests.Exceptions
{
    using System;
    using System.Text.Json.Nodes;

    /// <summary>
    /// An exception thrown during a <see cref="JsonNode"/> manipulation operation.
    /// </summary>
    public class JsonNodeException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JsonNodeException" /> class.
        /// </summary>
        /// <param name="message">The message indicating what went wrong.</param>
        public JsonNodeException(string message)
            : base(message)
        {
        }
    }
}