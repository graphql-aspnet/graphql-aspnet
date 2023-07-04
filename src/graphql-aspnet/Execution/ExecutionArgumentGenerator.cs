// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution
{
    using System;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.InputArguments;
    using GraphQL.AspNet.Interfaces.Execution.Variables;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// An object that can generate an <see cref="IExecutionArgumentCollection"/>
    /// from a set of arguments and variable values.
    /// </summary>
    public static class ExecutionArgumentGenerator
    {
        /// <summary>
        /// Attempts to resolve a set of input arguments, combined with a set of known variables, into a
        /// usable collection of values that can be submitted to a resolver.
        /// </summary>
        /// <param name="definedArguments">The defined arguments that need to be resolved.</param>
        /// <param name="variableData">The variable data to resolve against.</param>
        /// <param name="messages">The messages collection to fill with messages generated during the operation.</param>
        /// <param name="generatedCollection">When successful, this parameter
        /// will be populated with the generated collection.</param>
        /// <returns>IExecutionArgumentCollection.</returns>
        public static bool TryConvert(
            IInputArgumentCollection definedArguments,
            IResolvedVariableCollection variableData,
            IGraphMessageCollection messages,
            out IExecutionArgumentCollection generatedCollection)
        {
            generatedCollection = null;

            Validation.ThrowIfNull(definedArguments, nameof(definedArguments));
            Validation.ThrowIfNull(messages, nameof(messages));

            var successful = true;
            var collection = new ExecutionArgumentCollection(definedArguments.Count);
            foreach (var arg in definedArguments)
            {
                try
                {
                    var argDefinition = arg.Argument;
                    var resolvedValue = arg.Value.Resolve(variableData);

                    // its possible for a non-nullable variable to receive a
                    // null value due to a variable supplying null
                    // trap it and fail out the execution if so
                    // see: https://spec.graphql.org/October2021/#sel-GALbLHNCCBCGIp9O
                    if (resolvedValue == null && argDefinition.TypeExpression.IsNonNullable)
                    {
                        messages.Critical(
                          $"The value supplied to argument '{argDefinition.Name}' was <null> but its expected type expression " +
                          $"is {argDefinition.TypeExpression}.",
                          Constants.ErrorCodes.INVALID_ARGUMENT_VALUE,
                          arg.Origin);

                        successful = false;
                        continue;
                    }

                    collection.Add(new ExecutionArgument(arg.Argument, resolvedValue));
                }
                catch (UnresolvedValueException uve)
                {
                    // This may catch if there is a scenario where an input object
                    // has its fields constructed from nullable variables that
                    // are explicitly supplied a null value
                    //
                    // its also highly likely that at this stage there is no value resolver issue
                    // just a flat out failure. As a result append a semi-helpful message to the begining
                    // of the message.
                    var parentType = arg.Argument.Parent is IInputGraphField
                        ? "input field"
                        : "field";

                    messages.Critical(
                      $"The value supplied to argument '{arg.Name}' for {parentType} '{arg.Argument.Parent.Name}' was " +
                      $"not valid for the invocation. {uve.Message}",
                      Constants.ErrorCodes.INVALID_ARGUMENT_VALUE,
                      arg.Origin,
                      uve);

                    successful = false;
                }
                catch (Exception ex)
                {
                    var message = new GraphExecutionMessage(
                        GraphMessageSeverity.Critical,
                        "Invalid argument value. See exception for details.",
                        Constants.ErrorCodes.INVALID_ARGUMENT_VALUE,
                        arg.Origin,
                        ex);

                    successful = false;
                }
            }

            if (successful)
                generatedCollection = collection;

            return successful;
        }
    }
}