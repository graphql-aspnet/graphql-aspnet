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
    using GraphQL.AspNet.Logging.Common;
    using GraphQL.AspNet.Middleware.FieldExecution;

    /// <summary>
    /// Recorded by a field resolver when it starts resolving a field context and
    /// set of source items given to it. This occurs prior to the middleware pipeline being executed.
    /// </summary>
    public class FieldResolutionStartedLogEntry : GraphLogEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FieldResolutionStartedLogEntry" /> class.
        /// </summary>
        /// <param name="context">The field context containing the necessary data to resolve
        /// the field and produce a reslt.</param>
        public FieldResolutionStartedLogEntry(FieldResolutionContext context)
            : base(LogEventIds.FieldResolutionStarted)
        {
            this.PipelineRequestId = context.Request.Id;
            this.FieldExecutionMode = context.Request.Field.Mode.ToString();
            this.FieldPath = context.Request.Field.Route.Path;
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
        /// Gets the mode in which the field resolution is being processed.
        /// </summary>
        /// <value>The execution mode.</value>
        public string FieldExecutionMode
        {
            get => this.GetProperty<string>(LogPropertyNames.FIELD_EXECUTION_MODE);
            private set => this.SetProperty(LogPropertyNames.FIELD_EXECUTION_MODE, value);
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="string" /> that represents this instance.</returns>
        public override string ToString()
        {
            var idTruncated = this.PipelineRequestId?.Length > 8 ? this.PipelineRequestId.Substring(0, 8) : this.PipelineRequestId;
            return $"Field Resolution Started | Id: {idTruncated},  Path: {this.FieldPath}";
        }
    }
}