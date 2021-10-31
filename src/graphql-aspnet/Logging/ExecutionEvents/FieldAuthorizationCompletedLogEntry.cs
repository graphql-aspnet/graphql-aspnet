// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Logging.ExecutionEvents
{
    using System.Security.Claims;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Logging.Common;
    using GraphQL.AspNet.Middleware.FieldAuthorization;

    /// <summary>
    /// Recorded when the security middleware invokes a security challenge
    /// against a <see cref="ClaimsPrincipal"/>.
    /// </summary>
    public class FieldAuthorizationCompletedLogEntry : GraphLogEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FieldAuthorizationCompletedLogEntry" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public FieldAuthorizationCompletedLogEntry(GraphFieldAuthorizationContext context)
            : base(LogEventIds.FieldAuthorizationCompleted)
        {
            this.PipelineRequestId = context.Request.Id;
            this.FieldPath = context.Request.Field.Route.Path;
            this.Username = context.User?.RetrieveUsername();
            this.AuthorizationStatus = context.Result.Status.ToString();
            this.LogMessage = context.Result.LogMessage;
        }

        /// <summary>
        /// Gets the username of the user on the request, if any.
        /// </summary>
        /// <value>The username.</value>
        public string Username
        {
            get => this.GetProperty<string>(LogPropertyNames.USERNAME);
            private set => this.SetProperty(LogPropertyNames.USERNAME, value);
        }

        /// <summary>
        /// Gets a value indicating whether the security check passed and the user is authorized.
        /// </summary>
        /// <value><c>true</c> if this instance is authorized; otherwise, <c>false</c>.</value>
        public string AuthorizationStatus
        {
            get => this.GetProperty<string>(LogPropertyNames.SECURITY_AUTHORIZATION_STATUS);
            private set => this.SetProperty(LogPropertyNames.SECURITY_AUTHORIZATION_STATUS, value);
        }

        /// <summary>
        /// Gets the globally unique id that identifies the specific pipeline request
        /// that is being executed.
        /// </summary>
        /// <value>The pipeline request id.</value>
        public string PipelineRequestId
        {
            get => this.GetProperty<string>(LogPropertyNames.PIPELINE_REQUEST_ID);
            private set => this.SetProperty(LogPropertyNames.PIPELINE_REQUEST_ID, value);
        }

        /// <summary>
        /// Gets the fully qualified path in the graph schema that identifies the field
        /// being resolved.
        /// </summary>
        /// <value>The field path.</value>
        public string FieldPath
        {
            get => this.GetProperty<string>(LogPropertyNames.FIELD_PATH);
            private set => this.SetProperty(LogPropertyNames.FIELD_PATH, value);
        }

        /// <summary>
        /// Gets the log message generated during the authorization challenge.
        /// </summary>
        /// <value>The message, if any.</value>
        public string LogMessage
        {
            get => this.GetProperty<string>(LogPropertyNames.SECURITY_MESSAGES);
            private set => this.SetProperty(LogPropertyNames.SECURITY_MESSAGES, value);
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="string" /> that represents this instance.</returns>
        public override string ToString()
        {
            var idTruncated = this.PipelineRequestId?.Length > 8 ? this.PipelineRequestId.Substring(0, 8) : this.PipelineRequestId;
            return $"Field Auth Challenge Completed | Id: {idTruncated},  Path: '{this.FieldPath}', Status: {this.AuthorizationStatus} ";
        }
    }
}