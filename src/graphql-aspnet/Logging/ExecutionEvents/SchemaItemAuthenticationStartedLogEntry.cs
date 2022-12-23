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
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.Security;

    /// <summary>
    /// Recorded when the security middleware invokes an authentication challenge
    /// against a <see cref="IUserSecurityContext"/> for a given field.
    /// </summary>
    public class SchemaItemAuthenticationStartedLogEntry : GraphLogEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaItemAuthenticationStartedLogEntry" /> class.
        /// </summary>
        /// <param name="context">The auth context that is being resolved.</param>
        public SchemaItemAuthenticationStartedLogEntry(SchemaItemSecurityChallengeContext context)
            : base(LogEventIds.SchemaItemAuthenticationStarted)
        {
            this.PipelineRequestId = context?.Request?.Id.ToString();
            this.SchemaItemPath = context?.SecureSchemaItem?.Route?.Path;
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
        /// Gets the fully qualified path in the graph schema that identifies the item
        /// being resolved.
        /// </summary>
        /// <value>The schema item path.</value>
        public string SchemaItemPath
        {
            get => this.GetProperty<string>(LogPropertyNames.SCHEMA_ITEM_PATH);
            private set => this.SetProperty(LogPropertyNames.SCHEMA_ITEM_PATH, value);
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="string" /> that represents this instance.</returns>
        public override string ToString()
        {
            var idTruncated = this.PipelineRequestId?.Length > 8 ? this.PipelineRequestId.Substring(0, 8) : this.PipelineRequestId;
            return $"Field Authentication Started | Id: {idTruncated},  Path: '{this.SchemaItemPath}' ";
        }
    }
}