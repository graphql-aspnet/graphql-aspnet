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
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A generator to fully validate a set of arguments for a given request and generate a state dictionary with the results.
    /// </summary>
    public class ModelStateGenerator
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelStateGenerator" /> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider to use for generating validation contexts, if any.</param>
        public ModelStateGenerator(IServiceProvider serviceProvider = null)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Creates the state dictionary containing all the attribute base validation results for the provided arguments.
        /// </summary>
        /// <param name="argument">The argument to validate.</param>
        /// <returns>InputModelStateDictionary.</returns>
        public InputModelStateDictionary CreateStateDictionary(ExecutionArgument argument)
        {
            var dictionary = new InputModelStateDictionary();
            var entry = this.ValidateSingleArgument(argument);
            dictionary.Add(entry);

            return dictionary;
        }

        /// <summary>
        /// Creates the state dictionary containing all the attribute base validation results for the provided arguments.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <returns>InputModelStateDictionary.</returns>
        public InputModelStateDictionary CreateStateDictionary(IExecutionArgumentCollection arguments)
        {
            return this.CreateStateDictionary(arguments.Values);
        }

        /// <summary>
        /// Creates the state dictionary containing all the attribute base validation results for the provided arguments.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <returns>InputModelStateDictionary.</returns>
        public InputModelStateDictionary CreateStateDictionary(IEnumerable<ExecutionArgument> arguments)
        {
            var dictionary = new InputModelStateDictionary();
            if (arguments != null)
            {
                foreach (var argument in arguments)
                {
                    var entry = this.ValidateSingleArgument(argument);
                    dictionary.Add(entry);
                }
            }

            return dictionary;
        }

        /// <summary>
        /// Processes an input object's attribute validation items.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>InputModelStateEntry.</returns>
        private InputModelStateEntry ValidateSingleArgument(ExecutionArgument input)
        {
            var entry = new InputModelStateEntry(input);
            if (input.Value == null || input.Argument.ArgumentModifiers.IsSourceParameter())
            {
                entry.ValidationState = InputModelValidationState.Skipped;
                return entry;
            }

            if (input.Argument.TypeExpression.IsListOfItems)
            {
                if (!(input.Value is IEnumerable enumerable))
                {
                    // given the common pipeline validation this scenario is impossible
                    // (the plan generator would have caught this during query validation)
                    entry.ValidationState = InputModelValidationState.Invalid;
                    entry.AddErrorMessage("Argument should be a list.");
                }
                else
                {
                    foreach (var item in enumerable)
                        this.ValidateSingleItem(entry, item);
                }
            }
            else
            {
                this.ValidateSingleItem(entry, input.Value);
            }

            return entry;
        }

        /// <summary>
        /// Validates the single item's attribute validation parameters.
        /// </summary>
        /// <param name="entry">The entry.</param>
        /// <param name="inputValue">The input value.</param>
        private void ValidateSingleItem(InputModelStateEntry entry, object inputValue)
        {
            if (inputValue == null)
            {
                entry.ValidationState = InputModelValidationState.Skipped;
                return;
            }

            var results = new List<ValidationResult>();
            var context = new ValidationContext(inputValue, _serviceProvider, null);
            var isValid = Validator.TryValidateObject(inputValue, context, results, true);

            if (entry.ValidationState != InputModelValidationState.Invalid)
                entry.ValidationState = isValid ? InputModelValidationState.Valid : InputModelValidationState.Invalid;

            // extract all the errors that may have occured as a result of the validation operation
            if (!isValid)
            {
                foreach (var result in results)
                {
                    foreach (var memberName in result.MemberNames)
                    {
                        entry.AddErrorMessage(memberName, result.ErrorMessage);
                    }
                }
            }
        }
    }
}