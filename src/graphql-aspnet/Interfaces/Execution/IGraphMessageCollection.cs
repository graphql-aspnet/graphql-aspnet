// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Execution
{
    using System;
    using System.Collections.Generic;
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Execution;

    /// <summary>
    /// A collection of messages produced while completing a requested graph operation. Messages generated
    /// by the runtime or by custom code on field requests are aggregated and inspected for severity levels to
    /// deteremine if processing should cease or when a response needs to be sent to the request.
    /// </summary>
    public interface IGraphMessageCollection : IReadOnlyList<IGraphMessage>
    {
        /// <summary>
        /// Adds the specified message to the collection.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>IGraphExecutionMessage.</returns>
        IGraphMessage Add(IGraphMessage message);

        /// <summary>
        /// Adds the message to the collection.
        /// </summary>
        /// <param name="severity">The severity of this new message.</param>
        /// <param name="message">The body text for this new message.</param>
        /// <param name="errorCode">An optional error code to apply to the message.</param>
        /// <param name="origin">The origin in the source text where this message was generated.</param>
        /// <param name="exceptionThrown">An exception that may have been thrown and caused this message to be created.</param>
        /// <returns>IGraphExecutionMessage.</returns>
        IGraphMessage Add(GraphMessageSeverity severity, string message, string errorCode = "", SourceOrigin origin = null, Exception exceptionThrown = null);

        /// <summary>
        /// Adds the critical message to the collection.
        /// </summary>
        /// <param name="message">The body text for this new message.</param>
        /// <param name="errorCode">An optional error code to apply to the message.</param>
        /// <param name="origin">The origin in the source text where this message was generated.</param>
        /// <param name="exceptionThrown">An exception that may have been thrown and caused this message to be created.</param>
        /// <returns>IGraphExecutionMessage.</returns>
        IGraphMessage Critical(string message, string errorCode = "", SourceOrigin origin = null, Exception exceptionThrown = null);

        /// <summary>
        /// Adds the warning message to the collection.
        /// </summary>
        /// <param name="message">The body text for this new message.</param>
        /// <param name="errorCode">An optional error code to apply to the message..</param>
        /// <param name="origin">The origin in the source text where this message was generated.</param>
        /// <param name="exceptionThrown">An exception that may have been thrown and caused this message to be created.</param>
        /// <returns>IGraphExecutionMessage.</returns>
        IGraphMessage Warn(string message, string errorCode = "", SourceOrigin origin = null, Exception exceptionThrown = null);

        /// <summary>
        /// Adds the info message to the collection.
        /// </summary>
        /// <param name="message">The body text for this new message.</param>
        /// <param name="errorCode">An optional error code to apply to the message..</param>
        /// <param name="origin">The origin in the source text where this message was generated.</param>
        /// <param name="exceptionThrown">An exception that may have been thrown and caused this message to be created.</param>
        /// <returns>IGraphExecutionMessage.</returns>
        IGraphMessage Info(string message, string errorCode = "", SourceOrigin origin = null, Exception exceptionThrown = null);

        /// <summary>
        /// Adds the debug message to the collection.
        /// </summary>
        /// <param name="message">The body text for this new message.</param>
        /// <param name="errorCode">An optional error code to apply to the message..</param>
        /// <param name="origin">The origin in the source text where this message was generated.</param>
        /// <param name="exceptionThrown">An exception that may have been thrown and caused this message to be created.</param>
        /// <returns>IGraphExecutionMessage.</returns>
        IGraphMessage Debug(string message, string errorCode = "", SourceOrigin origin = null, Exception exceptionThrown = null);

        /// <summary>
        /// Adds the trace message to the collection.
        /// </summary>
        /// <param name="message">The body text for this new message.</param>
        /// <param name="errorCode">An optional error code to apply to the message..</param>
        /// <param name="origin">The origin in the source text where this message was generated.</param>
        /// <param name="exceptionThrown">An exception that may have been thrown and caused this message to be created.</param>
        /// <returns>IGraphExecutionMessage.</returns>
        IGraphMessage Trace(string message, string errorCode = "", SourceOrigin origin = null, Exception exceptionThrown = null);

        /// <summary>
        /// Adds the set of messages to the collection.
        /// </summary>
        /// <param name="messages">The messages.</param>
        void AddRange(IEnumerable<IGraphMessage> messages);

        /// <summary>
        /// Gets the highest level severity of all the messages tracked in this collection.
        /// </summary>
        /// <value>The severity.</value>
        GraphMessageSeverity Severity { get; }

        /// <summary>
        /// Gets a value indicating whether this collection of messages indicates a success (nothing critical).
        /// </summary>
        /// <value><c>true</c> if this instance is success; otherwise, <c>false</c>.</value>
        bool IsSucessful { get; }

        /// <summary>
        /// Clears all the messages in this instance.
        /// </summary>
        void Clear();
    }
}