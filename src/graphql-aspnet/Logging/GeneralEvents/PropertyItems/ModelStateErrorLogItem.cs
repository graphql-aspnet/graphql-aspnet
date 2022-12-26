﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Logging.GeneralEvents.PropertyItems
{
    using GraphQL.AspNet.Controllers.InputModel;
    using GraphQL.AspNet.Interfaces.Logging;

    /// <summary>
    /// A single error that occured during the validation of a model state item (usually
    /// geenrated by a single validation property).
    /// </summary>
    public class ModelStateErrorLogItem : GraphLogEntryPropertyCollection
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModelStateErrorLogItem"/> class.
        /// </summary>
        /// <param name="modelError">The model error.</param>
        public ModelStateErrorLogItem(InputModelError modelError)
        {
            this.ErrorMessage = modelError?.ErrorMessage;
            this.Exception = modelError?.Exception != null ? new ExceptionLogItem(modelError.Exception) : null;
        }

        /// <summary>
        /// Gets the custom error message applied to the generated error.
        /// </summary>
        /// <value>The model error.</value>
        public string ErrorMessage
        {
            get => this.GetProperty<string>(LogPropertyNames.MODEL_ITEM_NAME);
            private set => this.SetProperty(LogPropertyNames.MODEL_ITEM_NAME, value);
        }

        /// <summary>
        /// Gets the exception associated with this model error. May be null as not all errors
       /// are because of generated exceptions.
        /// </summary>
        /// <value>The exception.</value>
        public IGraphLogEntryPropertyCollection Exception
        {
            get => this.GetProperty<IGraphLogEntryPropertyCollection>(LogPropertyNames.EXCEPTION);
            private set => this.SetProperty(LogPropertyNames.EXCEPTION, value);
        }
    }
}