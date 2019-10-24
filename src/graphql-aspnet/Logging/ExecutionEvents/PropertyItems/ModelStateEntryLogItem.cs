// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Logging.ExecutionEvents.PropertyItems
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Execution.InputModel;
    using GraphQL.AspNet.Logging.Common;

    /// <summary>
    /// A collection of model state items that are part of a structured log entry.
    /// </summary>
    public class ModelStateEntryLogItem : GraphLogPropertyCollection
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModelStateEntryLogItem" /> class.
        /// </summary>
        /// <param name="modelStateItem">The model state item.</param>
        public ModelStateEntryLogItem(InputModelStateEntry modelStateItem)
        {
            this.Name = modelStateItem.Name;
            this.ValidationState = modelStateItem.ValidationState.ToString();

            if (modelStateItem.Errors != null && modelStateItem.Errors.Count > 0)
            {
                var errors = new List<ModelStateErrorLogItem>();

                foreach (var error in modelStateItem.Errors)
                {
                    errors.Add(new ModelStateErrorLogItem(error));
                }

                this.Errors = errors;
            }
        }

        /// <summary>
        /// Gets the name of the model state entry (usually the parameter name for the target
        /// controller name).
        /// </summary>
        /// <value>The model state name.</value>
        public string Name
        {
            get => this.GetProperty<string>(LogPropertyNames.MODEL_ITEM_NAME);
            private set => this.SetProperty(LogPropertyNames.MODEL_ITEM_NAME, value);
        }

        /// <summary>
        /// Gets the validation state of this model state entry. (valid, invalid etc.)
        /// </summary>
        /// <value>The validation state of the model item.</value>
        public string ValidationState
        {
            get => this.GetProperty<string>(LogPropertyNames.MODEL_ITEM_STATE);
            private set => this.SetProperty(LogPropertyNames.MODEL_ITEM_STATE, value);
        }

        /// <summary>
        /// Gets a collection of errors generated during the validation of the model item.
        /// </summary>
        /// <value>The errors collection.</value>
        public IList<ModelStateErrorLogItem> Errors
        {
            get => this.GetProperty<IList<ModelStateErrorLogItem>>(LogPropertyNames.MODEL_ITEM_ERRORS);
            private set => this.SetProperty(LogPropertyNames.MODEL_ITEM_ERRORS, value);
        }
    }
}