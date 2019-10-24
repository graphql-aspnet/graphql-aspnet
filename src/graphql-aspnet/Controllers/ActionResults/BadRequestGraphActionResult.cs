// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Controllers.ActionResults
{
    using System.Threading.Tasks;
    using GraphQL.AspNet.Execution.InputModel;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Middleware.FieldExecution;

    /// <summary>
    /// An action result indicating an the requested call to the controller
    /// was incorrect or otherwise unprocessable.
    /// </summary>
    /// <seealso cref="IGraphActionResult" />
    public class BadRequestGraphActionResult : IGraphActionResult
    {
        private readonly string _message;
        private readonly InputModelStateDictionary _modelState;

        /// <summary>
        /// Initializes a new instance of the <see cref="BadRequestGraphActionResult"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public BadRequestGraphActionResult(string message = null)
        {
            _message = message?.Trim() ?? string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BadRequestGraphActionResult"/> class.
        /// </summary>
        /// <param name="modelState">State of the model.</param>
        public BadRequestGraphActionResult(InputModelStateDictionary modelState)
        {
            _modelState = modelState;
        }

        /// <summary>
        /// Processes the provided resolution context against this action result instance to
        /// generate the expected response in accordance with this instance's rule set.
        /// </summary>
        /// <param name="resolutionContext">The resolution context.</param>
        /// <returns>Task.</returns>
        public Task Complete(ResolutionContext resolutionContext)
        {
            if (_modelState != null)
            {
                foreach (var entry in _modelState.Values)
                {
                    this.GenerateErrorMessages(resolutionContext, entry);
                }

                resolutionContext.Messages.Critical(
                    "Invalid model data.",
                    Constants.ErrorCodes.BAD_REQUEST,
                    resolutionContext.Request.Origin);
            }
            else if (!string.IsNullOrWhiteSpace(_message))
            {
                resolutionContext.Messages.Critical(
                    _message,
                    Constants.ErrorCodes.BAD_REQUEST,
                    resolutionContext.Request.Origin);
            }
            else
            {
                resolutionContext.Messages.Critical(
                    "Bad Request",
                    Constants.ErrorCodes.BAD_REQUEST,
                    resolutionContext.Request.Origin);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Generates the graphql friendly error messages for the model entry and adds them to the supplied builder.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="entry">The entry to extract messages from.</param>
        private void GenerateErrorMessages(ResolutionContext context, InputModelStateEntry entry)
        {
            foreach (var error in entry.Errors)
            {
                // membername in this case
                context.Messages.Critical(
                    error.ErrorMessage,
                    Constants.ErrorCodes.MODEL_VALIDATION_ERROR,
                    context.Request.Origin,
                    error.Exception);
            }
        }
    }
}