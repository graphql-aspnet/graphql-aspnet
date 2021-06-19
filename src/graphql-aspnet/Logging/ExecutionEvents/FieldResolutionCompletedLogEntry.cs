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
    using GraphQL.AspNet.Logging.Common;
    using GraphQL.AspNet.Middleware.FieldExecution;

    /// <summary>
    /// Recorded by a field resolver when it completes resolving a field context (and its children).
    /// This occurs after the middleware pipeline is executed.
    /// </summary>
    public class FieldResolutionCompletedLogEntry : GraphLogEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FieldResolutionCompletedLogEntry" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public FieldResolutionCompletedLogEntry(FieldResolutionContext context)
            : base(LogEventIds.FieldResolutionCompleted)
        {
            this.PipelineRequestId = context.Request.Id;
            this.FieldPath = context.Request.InvocationContext.Field.Route.Path;
            this.TypeExpression = context.Request.InvocationContext.Field.TypeExpression.ToString();
            this.HasData = context.Result != null;
            this.ResultIsValid = context.Messages.IsSucessful;
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
        /// Gets the expected type expression of the data returned on the field.
        /// </summary>
        /// <value>The field type expression.</value>
        public string TypeExpression
        {
            get => this.GetProperty<string>(LogPropertyNames.FIELD_TYPE_EXPRESSION);
            private set => this.SetProperty(LogPropertyNames.FIELD_TYPE_EXPRESSION, value);
        }

        /// <summary>
        /// Gets a value indicating whether data was generated and attached to the response.
        /// </summary>
        /// <value><c>true</c> if response data was generated; otherwise <c>false</c>.</value>
        public bool HasData
        {
            get => this.GetProperty<bool>(LogPropertyNames.FIELD_DATA_RETURNED);
            private set => this.SetProperty(LogPropertyNames.FIELD_DATA_RETURNED, value);
        }

        /// <summary>
        /// Gets a value indicating whether the result that was returned was valid and expected.
        /// </summary>
        /// <value><c>true</c> if response data was valid; otherwise <c>false</c>.</value>
        public bool ResultIsValid
        {
            get => this.GetProperty<bool>(LogPropertyNames.FIELD_RESULT_IS_VALID);
            private set => this.SetProperty(LogPropertyNames.FIELD_RESULT_IS_VALID, value);
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="string" /> that represents this instance.</returns>
        public override string ToString()
        {
            var idTruncated = this.PipelineRequestId?.Length > 8 ? this.PipelineRequestId.Substring(0, 8) : this.PipelineRequestId;
            return $"Field Resolution Completed | Id: {idTruncated},  Path: {this.FieldPath}, Successful: {this.ResultIsValid.ToString()}";
        }
    }
}