// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Apollo.Messages.ServerMessages
{
    using System;
    using GraphQL.AspNet.Apollo.Messages.Common;
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Execution;

    /// <summary>
    /// An apollo specific message containing one error that occured outside of any
    /// given graph query execution.
    /// </summary>
    public class ApolloServerErrorMessage : ApolloMessage<IGraphMessage>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApolloServerErrorMessage"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="code">The code.</param>
        /// <param name="severity">The severity.</param>
        /// <param name="exception">The exception.</param>
        public ApolloServerErrorMessage(
            string message,
            string code = null,
            GraphMessageSeverity severity = GraphMessageSeverity.Critical,
            Exception exception = null)
            : base(ApolloMessageType.ERROR)
        {
            this.Payload = new GraphExecutionMessage(severity, message, code, SourceOrigin.None, exception);
        }
    }
}