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
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.Security;
    using GraphQL.AspNet.Logging.Common;

    /// <summary>
    /// Recorded when the security middleware completes an authentication challenge
    /// against a <see cref="IUserSecurityContext"/>.
    /// </summary>
    public class FieldAuthenticationCompletedLogEntry : GraphLogEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FieldAuthenticationCompletedLogEntry" /> class.
        /// </summary>
        /// <param name="securityContext">The security context.</param>
        /// <param name="authResult">The authentication result.</param>
        public FieldAuthenticationCompletedLogEntry(GraphFieldSecurityContext securityContext, IAuthenticationResult authResult)
            : base(LogEventIds.FieldAuthenticationCompleted)
        {
            this.PipelineRequestId = securityContext?.Request.Id;
            this.FieldPath = securityContext?.Request.Field.Route.Path;
            this.Username = authResult?.User?.RetrieveUsername();
            this.AuthenticationScheme = authResult?.AuthenticationScheme;
            this.AuthethenticationSuccess = authResult?.Suceeded ?? false;
            this.LogMessage = securityContext?.Result?.LogMessage;
        }

        /// <summary>
        /// Gets the username of the user that was authenticated, if any.
        /// </summary>
        /// <value>The username.</value>
        public string Username
        {
            get => this.GetProperty<string>(LogPropertyNames.USERNAME);
            private set => this.SetProperty(LogPropertyNames.USERNAME, value);
        }

        /// <summary>
        /// Gets the authentication scheme of the handler that was used to authenticate the user.
        /// May be <c>null</c> if the default scheme was used.
        /// </summary>
        /// <value>The authentication scheme used.</value>
        public string AuthenticationScheme
        {
            get => this.GetProperty<string>(LogPropertyNames.AUTHENTICATION_SCHEME);
            private set => this.SetProperty(LogPropertyNames.AUTHENTICATION_SCHEME, value);
        }

        /// <summary>
        /// Gets a value indicating whether authentication was successful.
        /// </summary>
        /// <value><c>true</c> if [authethentication success]; otherwise, <c>false</c>.</value>
        public bool AuthethenticationSuccess
        {
            get => this.GetProperty<bool>(LogPropertyNames.AUTHENTICATION_SUCCESS);
            private set => this.SetProperty(LogPropertyNames.AUTHENTICATION_SUCCESS, value);
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
            return $"Field Authentication Completed | Id: {idTruncated},  Path: '{this.FieldPath}', Scheme: {this.AuthenticationScheme} | Successful: {this.AuthethenticationSuccess}";
        }
    }
}