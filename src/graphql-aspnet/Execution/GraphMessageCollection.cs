// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using GraphQL.AspNet.Common.Generics;
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Interfaces.Execution;

    /// <summary>
    /// A collection of messages produced while completing a requested graph operation. Messages generated
    /// by the runtime or by custom code on field requests are aggregated and inspected for severity levels to
    /// deteremine if processing should cease or when a response needs to be sent to the request.
    /// </summary>
    [DebuggerDisplay("Count = {Count}, Severity = {Severity}")]
    [DebuggerTypeProxy(typeof(GraphMessageCollectionDebugProxy))]
    [DebuggerStepThrough]
    [Serializable]
    public class GraphMessageCollection : IGraphMessageCollection
    {
        private readonly ConcurrentList<IGraphMessage> _messages;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphMessageCollection"/> class.
        /// </summary>
        public GraphMessageCollection()
        {
            _messages = new ConcurrentList<IGraphMessage>();
            _messages.ItemAdded += this.Message_ItemAdded;
            _messages.ListCleared += this.Messages_Cleared;
        }

        /// <summary>
        /// Called whenever the underlying list is fully cleared.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="message">The message that was removed (null for this operation).</param>
        private void Messages_Cleared(object sender, IGraphMessage message)
        {
            this.Severity = GraphMessageSeverity.Information;
        }

        /// <summary>
        /// Called whenever an item is addded the underlying collection.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="message">The message.</param>
        private void Message_ItemAdded(object sender, IGraphMessage message)
        {
            if (message.Severity > this.Severity)
                this.Severity = message.Severity;
        }

        /// <summary>
        /// Adds the specified message to the collection.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>IGraphExecutionMessage.</returns>
        public IGraphMessage Add(IGraphMessage message)
        {
            _messages.Add(message);
            return message;
        }

        /// <summary>
        /// Adds the message to the collection.
        /// </summary>
        /// <param name="severity">The severity of this new message.</param>
        /// <param name="message">The body text for this new message.</param>
        /// <param name="errorCode">An optional error code to apply to the message..</param>
        /// <param name="origin">The origin in the source text, if any, that this message relates to.</param>
        /// <param name="exceptionThrown">An exception that may have been thrown and caused this message to be created.</param>
        /// <returns>IGraphExecutionMessage.</returns>
        public IGraphMessage Add(
            GraphMessageSeverity severity,
            string message,
            string errorCode = "",
            SourceOrigin origin = null,
            Exception exceptionThrown = null)
        {
            var graphMessage = new GraphExecutionMessage(
                severity,
                message,
                errorCode,
                origin,
                exceptionThrown);

            return this.Add(graphMessage);
        }

        /// <summary>
        /// Adds the critical message to the collection.
        /// </summary>
        /// <param name="message">The body text for this new message.</param>
        /// <param name="errorCode">An optional error code to apply to the message..</param>
        /// <param name="origin">The origin in the source text, if any, that this message relates to.</param>
        /// <param name="exceptionThrown">An exception that may have been thrown and caused this message to be created.</param>
        /// <returns>IGraphExecutionMessage.</returns>
        public IGraphMessage Critical(
            string message,
            string errorCode = "",
            SourceOrigin origin = null,
            Exception exceptionThrown = null)
        {
            return this.Add(GraphMessageSeverity.Critical, message, errorCode, origin, exceptionThrown);
        }

        /// <summary>
        /// Adds the warning message to the collection.
        /// </summary>
        /// <param name="message">The body text for this new message.</param>
        /// <param name="errorCode">An optional error code to apply to the message..</param>
        /// <param name="origin">The origin in the source text, if any, that this message relates to.</param>
        /// <param name="exceptionThrown">An exception that may have been thrown and caused this message to be created.</param>
        /// <returns>IGraphExecutionMessage.</returns>
        public IGraphMessage Warn(
            string message,
            string errorCode = "",
            SourceOrigin origin = null,
            Exception exceptionThrown = null)
        {
            return this.Add(GraphMessageSeverity.Warning, message, errorCode, origin, exceptionThrown);
        }

        /// <summary>
        /// Adds the informational message to the collection.
        /// </summary>
        /// <param name="message">The body text for this new message.</param>
        /// <param name="errorCode">An optional error code to apply to the message..</param>
        /// <param name="origin">The origin in the source text, if any, that this message relates to.</param>
        /// <param name="exceptionThrown">An exception that may have been thrown and caused this message to be created.</param>
        /// <returns>IGraphExecutionMessage.</returns>
        public IGraphMessage Info(
            string message,
            string errorCode = "",
            SourceOrigin origin = null,
            Exception exceptionThrown = null)
        {
            return this.Add(GraphMessageSeverity.Information, message, errorCode, origin, exceptionThrown);
        }

        /// <summary>
        /// Adds the debug message to the collection.
        /// </summary>
        /// <param name="message">The body text for this new message.</param>
        /// <param name="errorCode">An optional error code to apply to the message..</param>
        /// <param name="origin">The origin in the source text where this message was generated.</param>
        /// <param name="exceptionThrown">An exception that may have been thrown and caused this message to be created.</param>
        /// <returns>IGraphExecutionMessage.</returns>
        public IGraphMessage Debug(
            string message,
            string errorCode = "",
            SourceOrigin origin = null,
            Exception exceptionThrown = null)
        {
            return this.Add(GraphMessageSeverity.Debug, message, errorCode, origin, exceptionThrown);
        }

        /// <summary>
        /// Adds the trace message to the collection.
        /// </summary>
        /// <param name="message">The body text for this new message.</param>
        /// <param name="errorCode">An optional error code to apply to the message..</param>
        /// <param name="origin">The origin in the source text where this message was generated.</param>
        /// <param name="exceptionThrown">An exception that may have been thrown and caused this message to be created.</param>
        /// <returns>IGraphExecutionMessage.</returns>
        public IGraphMessage Trace(
            string message,
            string errorCode = "",
            SourceOrigin origin = null,
            Exception exceptionThrown = null)
        {
            return this.Add(GraphMessageSeverity.Trace, message, errorCode, origin, exceptionThrown);
        }

        /// <summary>
        /// Adds the set of messages to the collection.
        /// </summary>
        /// <param name="messages">The messages.</param>
        public void AddRange(IEnumerable<IGraphMessage> messages)
        {
            if (messages == null)
                return;

            foreach (var message in messages)
                this.Add(message);
        }

        /// <summary>
        /// Clears all the messages in this instance.
        /// </summary>
        public void Clear()
        {
            _messages.Clear();
        }

        /// <summary>
        /// Gets the highest level severity of all the messages tracked in this collection.
        /// </summary>
        /// <value>The severity.</value>
        public GraphMessageSeverity Severity { get; private set;  }

        /// <summary>
        /// Gets the count of messages in this collection.
        /// </summary>
        /// <value>The count.</value>
        public int Count => _messages.Count;

        /// <summary>
        /// Gets a value indicating whether this collection of messages indicates a success (nothing critical).
        /// </summary>
        /// <value><c>true</c> if this instance is success; otherwise, <c>false</c>.</value>
        public bool IsSucessful => !this.Severity.IsCritical();

        /// <summary>
        /// Gets the <see cref="IGraphMessage"/> at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>IGraphExecutionMessage.</returns>
        public IGraphMessage this[int index] => _messages[index];

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<IGraphMessage> GetEnumerator()
        {
            return _messages.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}