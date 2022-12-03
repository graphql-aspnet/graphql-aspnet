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
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Interfaces.Execution;

    /// <summary>
    /// A collection of messages produced while completing a requested graph operation. Messages generated
    /// by the runtime or by custom code on field requests are aggregated and inspected for severity levels to
    /// deteremine if processing should cease or when a response needs to be sent to the requestor.
    /// </summary>
    [DebuggerDisplay("Count = {Count}, Severity = {Severity}")]
    [DebuggerTypeProxy(typeof(GraphMessageCollectionDebugProxy))]
    [DebuggerStepThrough]
    public class GraphMessageCollection : IGraphMessageCollection
    {
        private List<IGraphMessage> _messages;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphMessageCollection"/> class.
        /// </summary>
        public GraphMessageCollection()
        {
            _messages = new List<IGraphMessage>();
        }

        /// <inheritdoc />
        public void AddRange(IGraphMessageCollection messagesToAdd)
        {
            if (messagesToAdd == null || messagesToAdd.Count == 0)
                return;

            lock (_messages)
            {
                // instead of letting the list dynamicly size itself
                // as messages are added in, ensure its only ever resized
                // once for the whole operation.
                var newCount = messagesToAdd.Count + _messages.Count;
                if (newCount < _messages.Capacity)
                {
                    var newCapacity = _messages.Capacity * 2;
                    while (newCount < newCapacity)
                        newCapacity = newCapacity * 2;

                    _messages.Capacity = newCapacity;
                }

                for (var i = 0; i < messagesToAdd.Count; i++)
                {
                    _messages.Add(messagesToAdd[i]);

                    if (messagesToAdd[i].Severity > this.Severity)
                        this.Severity = messagesToAdd[i].Severity;
                }
            }
        }

        /// <inheritdoc />
        public IGraphMessage Add(IGraphMessage message)
        {
            lock (_messages)
            {
                _messages.Add(message);

                if (message.Severity > this.Severity)
                    this.Severity = message.Severity;
            }

            return message;
        }

        /// <inheritdoc />
        public IGraphMessage Add(
            GraphMessageSeverity severity,
            string message,
            string errorCode = "",
            SourceOrigin origin = default,
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

        /// <inheritdoc />
        public IGraphMessage Critical(
            string message,
            string errorCode = "",
            SourceOrigin origin = default,
            Exception exceptionThrown = null)
        {
            return this.Add(GraphMessageSeverity.Critical, message, errorCode, origin, exceptionThrown);
        }

        /// <inheritdoc />
        public IGraphMessage Warn(
            string message,
            string errorCode = "",
            SourceOrigin origin = default,
            Exception exceptionThrown = null)
        {
            return this.Add(GraphMessageSeverity.Warning, message, errorCode, origin, exceptionThrown);
        }

        /// <inheritdoc />
        public IGraphMessage Info(
            string message,
            string errorCode = "",
            SourceOrigin origin = default,
            Exception exceptionThrown = null)
        {
            return this.Add(GraphMessageSeverity.Information, message, errorCode, origin, exceptionThrown);
        }

        /// <inheritdoc />
        public IGraphMessage Debug(
            string message,
            string errorCode = "",
            SourceOrigin origin = default,
            Exception exceptionThrown = null)
        {
            return this.Add(GraphMessageSeverity.Debug, message, errorCode, origin, exceptionThrown);
        }

        /// <inheritdoc />
        public IGraphMessage Trace(
            string message,
            string errorCode = "",
            SourceOrigin origin = default,
            Exception exceptionThrown = null)
        {
            return this.Add(GraphMessageSeverity.Trace, message, errorCode, origin, exceptionThrown);
        }

        /// <inheritdoc />
        public GraphMessageSeverity Severity { get; private set; }

        /// <inheritdoc />
        public int Count => _messages.Count;

        /// <inheritdoc />
        public bool IsSucessful => !this.Severity.IsCritical();

        /// <inheritdoc />
        public IGraphMessage this[int index]
        {
            get
            {
                lock (_messages)
                    return _messages[index];
            }
        }

        /// <inheritdoc />
        public IEnumerator<IGraphMessage> GetEnumerator()
        {
            lock (_messages)
                return _messages.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}