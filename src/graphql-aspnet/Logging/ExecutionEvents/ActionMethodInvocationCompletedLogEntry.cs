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
    using System;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Logging.Common;

    /// <summary>
    /// Recorded after a controller invokes and receives a result from an action method.
    /// </summary>
    public class ActionMethodInvocationCompletedLogEntry : GraphLogEntry
    {
        private readonly string _shortControllerName;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionMethodInvocationCompletedLogEntry" /> class.
        /// </summary>
        /// <param name="method">The method being invoked.</param>
        /// <param name="request">The request being executed on the method.</param>
        /// <param name="result">The result that was generated.</param>
        public ActionMethodInvocationCompletedLogEntry(IGraphMethod method, IDataRequest request, object result)
            : base(LogEventIds.ControllerInvocationCompleted)
        {
            this.PipelineRequestId = request.Id;
            this.ControllerName = method.Parent.InternalFullName;
            this.ActionName = method.InternalName;
            this.FieldPath = method.Route.Path;
            this.ResultTypeName = result?.GetType().FriendlyName(true);
            _shortControllerName = method.Parent.InternalName;
        }

        /// <summary>
        /// Gets the globally unique id that identifies the specific pipeline request
        /// that is being executed.
        /// </summary>
        /// <value>The message.</value>
        public string PipelineRequestId
        {
            get => this.GetProperty<string>(LogPropertyNames.PIPELINE_REQUEST_ID);
            private set => this.SetProperty(LogPropertyNames.PIPELINE_REQUEST_ID, value);
        }

        /// <summary>
        /// Gets the internal name of the controller being invoked.
        /// </summary>
        /// <value>The controller name.</value>
        public string ControllerName
        {
            get => this.GetProperty<string>(LogPropertyNames.CONTROLLER_TYPE_NAME);
            private set => this.SetProperty(LogPropertyNames.CONTROLLER_TYPE_NAME, value);
        }

        /// <summary>
        /// Gets the internal name of the action, on the controller, being invoked.
        /// </summary>
        /// <value>The action name.</value>
        public string ActionName
        {
            get => this.GetProperty<string>(LogPropertyNames.ACTION_NAME);
            private set => this.SetProperty(LogPropertyNames.ACTION_NAME, value);
        }

        /// <summary>
        /// Gets the path, in the target schema, of the action.
        /// </summary>
        /// <value>The action name.</value>
        public string FieldPath
        {
            get => this.GetProperty<string>(LogPropertyNames.FIELD_PATH);
            private set => this.SetProperty(LogPropertyNames.FIELD_PATH, value);
        }

         /// <summary>
        /// Gets the <see cref="Type"/> name of the data that was returned. If the method
        /// returned a <see cref="IGraphActionResult"/> the type of the action result is recorded.
        /// action.
        /// </summary>
        /// <value>The action source data type.</value>
        public string ResultTypeName
        {
            get => this.GetProperty<string>(LogPropertyNames.ACTION_RESULT_TYPE_NAME);
            private set => this.SetProperty(LogPropertyNames.ACTION_RESULT_TYPE_NAME, value);
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="string" /> that represents this instance.</returns>
        public override string ToString()
        {
            var idTruncated = this.PipelineRequestId?.Length > 8 ? this.PipelineRequestId.Substring(0, 8) : this.PipelineRequestId;
            return $"Action Invocation Completed | Id: {idTruncated}, Method: {_shortControllerName}.{this.ActionName}, Has Data: {this.ResultTypeName != null}";
        }
    }
}