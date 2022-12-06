// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.Mock
{
    using System;

    /// <summary>
    /// A message that when dequeued by a connection will invoke the given action.
    /// This message will not be received by the connected client proxy.
    /// </summary>
    public class MockTestActionMessage : MockSocketMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MockTestActionMessage"/> class.
        /// </summary>
        /// <param name="action">The action.</param>
        public MockTestActionMessage(Action action)
            : base(string.Empty)
        {
            this.Action = action;
        }

        /// <summary>
        /// Gets the action to execute.
        /// </summary>
        /// <value>The action.</value>
        public Action Action { get; }
    }
}