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
    using GraphQL.AspNet.Logging.Common;
    using GraphQL.AspNet.Middleware.FieldAuthorization;

    /// <summary>
    /// Recorded when the security middleware invokes a security challenge
    /// against a <see cref="ClaimsPrincipal"/>.
    /// </summary>
    public class FieldAuthorizationStartedLogEntry : GraphLogEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FieldAuthorizationStartedLogEntry" /> class.
        /// </summary>
        /// <param name="context">The auth context that is being resolved.</param>
        public FieldAuthorizationStartedLogEntry(GraphFieldAuthorizationContext context)
            : base(LogEventIds.FieldAuthorizationStarted)
        {
            this.PipelineRequestId = context.Request.Id;
            this.FieldPath = context.Field.Route.Path;
            this.Username = context.User?.RetrieveUsername();
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
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="string" /> that represents this instance.</returns>
        public override string ToString()
        {
            var idTruncated = this.PipelineRequestId?.Length > 8 ? this.PipelineRequestId.Substring(0, 8) : this.PipelineRequestId;
            return $"Field Auth Challenge Started | Id: {idTruncated},  Path: '{this.FieldPath}' ";
        }
    }
}