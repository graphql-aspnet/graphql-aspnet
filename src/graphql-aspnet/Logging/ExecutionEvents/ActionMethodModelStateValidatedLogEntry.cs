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
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution.InputModel;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Logging.Common;
    using GraphQL.AspNet.Logging.ExecutionEvents.PropertyItems;

    /// <summary>
    /// Recorded when a controller completes validation of the model data that will be passed
    /// to the action method.
    /// </summary>
    public class ActionMethodModelStateValidatedLogEntry : GraphLogEntry
    {
        private readonly string _shortControllerName;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionMethodModelStateValidatedLogEntry" /> class.
        /// </summary>
        /// <param name="method">The method being invoked.</param>
        /// <param name="request">The request being executed on the method.</param>
        /// <param name="modelState">the model dictionary created by the controller.</param>
        public ActionMethodModelStateValidatedLogEntry(
            IGraphMethod method,
            IDataRequest request,
            InputModelStateDictionary modelState)
            : base(LogEventIds.ControllerModelValidated)
        {
            this.PipelineRequestId = request.Id;
            this.ControllerName = method.Parent.ObjectType?.FriendlyName(true) ?? method.Parent.Name;
            this.ActionName = method.Name;
            this.FieldPath = method.Route.Path;
            this.ModelDataIsValid = modelState.IsValid;
            _shortControllerName = method.Parent.ObjectType?.FriendlyName() ?? method.Parent.Name;
            this.ModelItems = null;
            if (modelState.Values != null && modelState.Values.Any())
            {
                var entries = new List<IGraphLogPropertyCollection>();
                foreach (var item in modelState.Values)
                {
                    if (item.ValidationState == InputModelValidationState.Invalid)
                    {
                        entries.Add(new ModelStateEntryLogItem(item));
                    }
                }

                this.ModelItems = entries;
            }
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
        /// Gets a value indicating whether the collective sum of model data is in a valid state
        /// when its passed to the method for execution.
        /// </summary>
        /// <value><c>true</c> if the model data is valid; otherwise <c>false</c>.</value>
        public bool ModelDataIsValid
        {
            get => this.GetProperty<bool>(LogPropertyNames.ACTION_MODEL_DATA_IS_VALID);
            private set => this.SetProperty(LogPropertyNames.ACTION_MODEL_DATA_IS_VALID, value);
        }

        /// <summary>
        /// Gets the collection of model items that were validated.
        /// </summary>
        /// <value>The model items.</value>
        public IList<IGraphLogPropertyCollection> ModelItems
        {
            get => this.GetProperty<IList<IGraphLogPropertyCollection>>(LogPropertyNames.MODEL_ITEMS_COLLECTION);
            private set => this.SetProperty(LogPropertyNames.MODEL_ITEMS_COLLECTION, value);
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="string" /> that represents this instance.</returns>
        public override string ToString()
        {
            var idTruncated = this.PipelineRequestId?.Length > 8 ? this.PipelineRequestId.Substring(0, 8) : this.PipelineRequestId;
            return $"Action Model State Validated | Id: {idTruncated}, Method: {_shortControllerName}.{this.ActionName}, Is Valid: {this.ModelDataIsValid}";
        }
    }
}