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
    using System.Collections.Generic;
    using System.Diagnostics;
    using GraphQL.AspNet.Interfaces.Execution;

    /// <summary>
    /// A visual studio helper class for debugging through <see cref="GraphMessageCollection"/>.
    /// </summary>
    [DebuggerDisplay("Count = {Count}, Severity = {Severity}")]
    internal class GraphMessageCollectionDebugProxy
    {
        private readonly GraphMessageCollection _messageCollection;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphMessageCollectionDebugProxy"/> class.
        /// </summary>
        /// <param name="messageCollection">The message collection.</param>
        public GraphMessageCollectionDebugProxy(GraphMessageCollection messageCollection)
        {
            _messageCollection = messageCollection;
            this.Messages = new List<IGraphMessage>(_messageCollection);
        }

        /// <summary>
        /// Gets the count of messages in the collection.
        /// </summary>
        /// <value>The count.</value>
        public int Count => _messageCollection.Count;

        /// <summary>
        /// Gets the severity of the collection.
        /// </summary>
        /// <value>The severity.</value>
        public GraphMessageSeverity Severity => _messageCollection.Severity;

        /// <summary>
        /// Gets the messages on the collection.
        /// </summary>
        /// <value>The messages.</value>
        public IReadOnlyList<IGraphMessage> Messages { get; }
    }
}