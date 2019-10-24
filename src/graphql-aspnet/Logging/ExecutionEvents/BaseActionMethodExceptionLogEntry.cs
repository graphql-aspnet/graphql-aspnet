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
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Logging.Common;
    using GraphQL.AspNet.Logging.ExecutionEvents.PropertyItems;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// A base entry with common info about an exception that was thrown while
    /// executing a controller action method.
    /// </summary>
    public abstract class BaseActionMethodExceptionLogEntry : GraphLogEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseActionMethodExceptionLogEntry" /> class.
        /// </summary>
        /// <param name="eventId">The event identifier.</param>
        /// <param name="method">The method being invoked.</param>
        /// <param name="request">The request being executed on the method.</param>
        /// <param name="exception">The exception that was thrown.</param>
        protected BaseActionMethodExceptionLogEntry(
            EventId eventId,
            IGraphMethod method,
            IDataRequest request,
            Exception exception)
            : base(eventId)
        {
            this.PipelineRequestId = request.Id;
            this.ControllerTypeName = method.Parent.InternalFullName;
            this.ActionName = method.Name;
            this.Exception = new ExceptionLogItem(exception);
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
        public string ControllerTypeName
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
        /// Gets the message of the exception that was thrown.
        /// </summary>
        /// <value>The exception message.</value>
        public ExceptionLogItem Exception
        {
            get => this.GetProperty<ExceptionLogItem>(LogPropertyNames.EXCEPTION);
            private set => this.SetProperty(LogPropertyNames.EXCEPTION, value);
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="string" /> that represents this instance.</returns>
        public override string ToString()
        {
            return $"Action Exception | Type: '{this.Exception.ShortTypeName}', Message: '{this.Exception.ExceptionMessage}' ";
        }
    }
}