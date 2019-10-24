// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Security
{
    using System.Diagnostics;
    using GraphQL.AspNet.Interfaces.Execution;

    /// <summary>
    /// A result generated after testing a <see cref="IGraphOperationRequest"/> ability to access
    /// a requested field of data.
    /// </summary>
    [DebuggerDisplay("Status: {Status}")]
    public class FieldAuthorizationResult
    {
         /// <summary>
        /// Creates an authorization result indicating that the user was successfully authorized to the field.
        /// </summary>
        /// <returns>FieldAuthorizationResult.</returns>
        public static FieldAuthorizationResult Success()
        {
            return new FieldAuthorizationResult(FieldAuthorizationStatus.Authorized);
        }

        /// <summary>
        /// Creates an authorization result indicating that no authorization was necessary.
        /// </summary>
        /// <returns>FieldAuthorizationResult.</returns>
        public static FieldAuthorizationResult Skipped()
        {
            return new FieldAuthorizationResult(FieldAuthorizationStatus.Skipped);
        }

        /// <summary>
        /// Creates an authorization result indicating that authorization failed, along with an optional, internally scoped
        /// message.
        /// </summary>
        /// <param name="internalMessage">The internal message.</param>
        /// <returns>FieldAuthorizationResult.</returns>
        public static FieldAuthorizationResult Fail(string internalMessage)
        {
            return new FieldAuthorizationResult(FieldAuthorizationStatus.Unauthorized, internalMessage);
        }

        /// <summary>
        /// Creates a default, failed result with a generic message indicating no other result existed.
        /// </summary>
        /// <returns>FieldAuthorizationResult.</returns>
        public static FieldAuthorizationResult Default()
        {
            return new FieldAuthorizationResult(FieldAuthorizationStatus.Unauthorized, "Unknown result");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldAuthorizationResult" /> class.
        /// </summary>
        private FieldAuthorizationResult(FieldAuthorizationStatus status, string message = null)
        {
            this.LogMessage = message;
            this.Status = status;
        }

        /// <summary>
        /// Gets the status of this result.
        /// </summary>
        /// <value>The status.</value>
        public FieldAuthorizationStatus Status { get; }

        /// <summary>
        /// Gets a collection of log-based messages to be recorded to any log events. May
        /// contain sensitive data and should not be sent to a requestor.
        /// </summary>
        /// <value>The log messages.</value>
        public string LogMessage { get; }
    }
}