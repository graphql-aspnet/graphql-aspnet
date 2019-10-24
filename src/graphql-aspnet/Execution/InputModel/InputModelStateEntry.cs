// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.InputModel
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using GraphQL.AspNet.Common;

    /// <summary>
    /// An entry in a model state dictionary describing the internal (non-graphql) validation of an input argument to an action method.
    /// </summary>
    [DebuggerDisplay("{Name}, Errors = {Errors.Count}")]
    public class InputModelStateEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InputModelStateEntry"/> class.
        /// </summary>
        /// <param name="inputArgument">The input argument.</param>
        public InputModelStateEntry(ExecutionArgument inputArgument)
        {
            this.Model = Validation.ThrowIfNullOrReturn(inputArgument, nameof(inputArgument));
            this.ValidationState = InputModelValidationState.Unvalidated;
            this.Errors = new List<InputModelError>();
        }

        /// <summary>
        /// Adds the error message to this state entry for a specific member (property) of the argment. Adding an error
        /// message will immediately flip the validation state of this entry to invalid.
        /// </summary>
        /// <param name="memberName">Name of the member.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>InputModelError.</returns>
        public InputModelError AddErrorMessage(string memberName, string errorMessage)
        {
            var error = new InputModelError(memberName, errorMessage);
            this.Errors.Add(error);
            this.ValidationState = InputModelValidationState.Invalid;
            return error;
        }

        /// <summary>
        /// Adds a general error message to this state entry.  dding an error
        /// message will immediately flip the validation state of this entry to invalid.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>InputModelError.</returns>
        public InputModelError AddErrorMessage(string errorMessage)
        {
            var error = new InputModelError(errorMessage);
            this.Errors.Add(error);
            this.ValidationState = InputModelValidationState.Invalid;
            return error;
        }

        /// <summary>
        /// Gets the server side name of the argument that has been validated, as it was defined on the server side code.
        /// </summary>
        /// <value>The name.</value>
        public string Name => this.Model.Argument.ParameterName;

        /// <summary>
        /// Gets the model item that generated this state entry.
        /// </summary>
        /// <value>The model.</value>
        public ExecutionArgument Model { get; }

        /// <summary>
        /// Gets the errors assigned to this state entry.
        /// </summary>
        /// <value>The errors.</value>
        public IList<InputModelError> Errors { get; }

        /// <summary>
        /// Gets or sets the validation state of this instance.
        /// </summary>
        /// <value>The state of the validation.</value>
        public InputModelValidationState ValidationState { get; set; }
    }
}