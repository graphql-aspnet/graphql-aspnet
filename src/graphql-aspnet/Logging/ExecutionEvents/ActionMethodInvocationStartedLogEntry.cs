﻿// *************************************************************
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
    using GraphQL.AspNet.Interfaces.Execution;

    /// <summary>
    /// Recorded when a new request is generated by a query controller and passed to an
    /// executor for processing. This event is recorded before any action is taken.
    /// </summary>
    public class ActionMethodInvocationStartedLogEntry : GraphLogEntry
    {
        private readonly string _shortControllerName;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionMethodInvocationStartedLogEntry" /> class.
        /// </summary>
        /// <param name="method">The method being invoked.</param>
        /// <param name="request">The request being executed on the method.</param>
        public ActionMethodInvocationStartedLogEntry(IGraphMethod method, IDataRequest request)
            : base(LogEventIds.ControllerInvocationStarted)
        {
            this.PipelineRequestId = request?.Id.ToString();
            this.ControllerName = method?.Parent?.InternalFullName;
            this.ActionName = method?.Name;
            this.FieldPath = method?.Route?.Path;
            this.SourceObjectType = method?.ObjectType?.ToString();
            this.IsAsync = method?.IsAsyncField;
            _shortControllerName = method?.Parent?.InternalName;
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
            get => this.GetProperty<string>(LogPropertyNames.SCHEMA_ITEM_PATH);
            private set => this.SetProperty(LogPropertyNames.SCHEMA_ITEM_PATH, value);
        }

         /// <summary>
        /// Gets the <see cref="Type"/> name of the expected source data object to this
        /// action.
        /// </summary>
        /// <value>The action source data type.</value>
        public string SourceObjectType
        {
            get => this.GetProperty<string>(LogPropertyNames.ACTION_SOURCE_OBJECT_TYPE);
            private set => this.SetProperty(LogPropertyNames.ACTION_SOURCE_OBJECT_TYPE, value);
        }

          /// <summary>
        /// Gets a value indicating whether this action method is being executed asyncronously
        /// or not
        /// action.
        /// </summary>
        /// <value><c>true</c> if the action method is an async method; otherwise <c>false</c>.</value>
        public bool? IsAsync
        {
            get => this.GetProperty<bool?>(LogPropertyNames.ACTION_IS_ASYNC);
            private set => this.SetProperty(LogPropertyNames.ACTION_IS_ASYNC, value);
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="string" /> that represents this instance.</returns>
        public override string ToString()
        {
            var idTruncated = this.PipelineRequestId?.Length > 8 ? this.PipelineRequestId.Substring(0, 8) : this.PipelineRequestId;
            return $"Action Invocation Started | Id: {idTruncated}, Method: {_shortControllerName}.{this.ActionName}";
        }
    }
}