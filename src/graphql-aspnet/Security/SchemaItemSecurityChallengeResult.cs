﻿// *************************************************************
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
    using System.Security.Claims;
    using GraphQL.AspNet.Interfaces.Security;

    /// <summary>
    /// A result generated after verifying the ability of a <see cref="IUserSecurityContext"/>
    /// to access a requested schema item.
    /// </summary>
    [DebuggerDisplay("Status: {Status}")]
    public sealed class SchemaItemSecurityChallengeResult
    {
        /// <summary>
        /// Creates an authorization result indicating that the user was successfully
        /// authorized to the field.
        /// </summary>
        /// <param name="user">The user that was authorized by the challenge.</param>
        /// <returns>SchemaItemSecurityChallengeResult.</returns>
        public static SchemaItemSecurityChallengeResult Success(
            ClaimsPrincipal user)
        {
            return new SchemaItemSecurityChallengeResult(
                SchemaItemSecurityChallengeStatus.Authorized,
                user);
        }

        /// <summary>
        /// Creates an authorization result indicating that no authorization was necessary.
        /// </summary>
        /// <param name="user">The user credentials present on the security context, if any,
        /// at the time it was determined that authorization could be skipped.</param>
        /// <returns>SchemaItemSecurityChallengeResult.</returns>
        public static SchemaItemSecurityChallengeResult Skipped(ClaimsPrincipal user)
        {
            return new SchemaItemSecurityChallengeResult(SchemaItemSecurityChallengeStatus.Skipped, user);
        }

        /// <summary>
        /// Creates an authorization result indicating that authorization failed
        /// to complete, along with an optional, internally scoped message.
        /// </summary>
        /// <param name="internalMessage">An internal message indicating why the challenge
        /// failed. This message may end up in log entries.</param>
        /// <returns>SchemaItemSecurityChallengeResult.</returns>
        public static SchemaItemSecurityChallengeResult Fail(string internalMessage)
        {
            return new SchemaItemSecurityChallengeResult(
                SchemaItemSecurityChallengeStatus.Failed,
                message: internalMessage);
        }

        /// <summary>
        /// Creates an authorization result indicating that authorization completed but hte user
        /// was unauthorized, along with an optional, internally scoped
        /// message.
        /// </summary>
        /// <param name="internalMessage">An internal message indicating why the challenge
        /// failed. This message may end up in log entries.</param>
        /// <returns>SchemaItemSecurityChallengeResult.</returns>
        public static SchemaItemSecurityChallengeResult Unauthorized(string internalMessage)
        {
            return new SchemaItemSecurityChallengeResult(
                SchemaItemSecurityChallengeStatus.Unauthorized,
                message: internalMessage);
        }

        /// <summary>
        /// Creates an authorization result indicating that authorization completed but the user
        /// was unauthenticated based on a required authentication scheme.
        /// message.
        /// </summary>
        /// <param name="internalMessage">An internal message indicating why the challenge
        /// failed. This message may end up in log entries.</param>
        /// <returns>SchemaItemSecurityChallengeResult.</returns>
        public static SchemaItemSecurityChallengeResult UnAuthenticated(string internalMessage)
        {
            return new SchemaItemSecurityChallengeResult(
                SchemaItemSecurityChallengeStatus.Unauthenticated,
                message: internalMessage);
        }

        /// <summary>
        /// Creates a default, failed result with a generic message indicating no other
        /// result existed.
        /// </summary>
        /// <returns>SchemaItemSecurityChallengeResult.</returns>
        public static SchemaItemSecurityChallengeResult Default()
        {
            return new SchemaItemSecurityChallengeResult(
                SchemaItemSecurityChallengeStatus.Unauthorized,
                message: "Unknown result");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaItemSecurityChallengeResult" /> class.
        /// </summary>
        private SchemaItemSecurityChallengeResult(
            SchemaItemSecurityChallengeStatus status,
            ClaimsPrincipal user = null,
            string message = null)
        {
            this.LogMessage = message;
            this.Status = status;
            this.User = user;
        }

        /// <summary>
        /// Gets the status of this result.
        /// </summary>
        /// <value>The status.</value>
        public SchemaItemSecurityChallengeStatus Status { get; }

        /// <summary>
        /// Gets the user principal that was constructed from the authorized security context.
        /// May be null if no user was present on the security context or if a given user was not
        /// necessary to authorize the field such as when skipping authorization entirely.
        /// </summary>
        /// <value>The user.</value>
        public ClaimsPrincipal User { get; }

        /// <summary>
        /// Gets a log friendly message to be recorded to any log events. May
        /// contain sensitive data and should not be sent to a requestor.
        /// </summary>
        /// <value>The log messages.</value>
        public string LogMessage { get; }
    }
}