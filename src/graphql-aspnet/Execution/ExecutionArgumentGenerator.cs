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
    using GraphQL.AspNet.Execution.QueryPlans.InputArguments;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.InputArguments;
    using GraphQL.AspNet.Interfaces.Execution.Variables;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// An object that can generate an <see cref="IExecutionArgumentCollection"/>
    /// from a set of arguments and variable values.
    /// </summary>
    public class ExecutionArgumentGenerator
    {
        private readonly IInputArgumentCollection _arguments;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionArgumentGenerator" /> class.
        /// </summary>
        /// <param name="argumentCollection">The argument collection defined
        /// on some query operation that needs to be converted to execution arguments.</param>
        /// <param name="messages">If provided, the set of messages to be appended
        /// to with any messages generated via the use of this instance.</param>
        public ExecutionArgumentGenerator(
            IInputArgumentCollection argumentCollection,
            IGraphMessageCollection messages = null)
        {
            _arguments = Validation.ThrowIfNullOrReturn(argumentCollection, nameof(argumentCollection));
            this.Messages = messages ?? new GraphMessageCollection();
        }

        /// <summary>
        /// Combines the supplied set of variables with the contained arguments
        /// to generate a set of arguments that can be used to execute a resolver.
        /// </summary>
        /// <param name="variableData">The variable data to process.</param>
        /// <param name="generatedCollection">When successful, this parameter
        /// will be populated with the generated collection.</param>
        /// <returns>IExecutionArgumentCollection.</returns>
        public bool TryConvert(IResolvedVariableCollection variableData, out IExecutionArgumentCollection generatedCollection)
        {
            generatedCollection = null;

            var successful = true;
            var collection = new ExecutionArgumentCollection(_arguments.Count);
            foreach (var arg in _arguments)
            {
                try
                {
                    var argDefinition = arg.Argument;
                    var resolvedValue = arg.Value.Resolve(variableData);

                    // no schema arguments are internally controlled
                    // the resolved value is the value we want
                    if (argDefinition.ArgumentModifiers.IsPartOfTheSchema())
                    {
                        // its possible for a non-nullable variable to receive a
                        // null value due to a variable supplying null
                        // trap it and fail out the execution if so
                        // see: https://spec.graphql.org/October2021/#sel-GALbLHNCCBCGIp9O
                        if (resolvedValue == null && argDefinition.TypeExpression.IsNonNullable)
                        {
                            this.Messages.Critical(
                              $"The value supplied to argument '{argDefinition.Name}' was <null> but its expected type expression " +
                              $"is {argDefinition.TypeExpression}.",
                              Constants.ErrorCodes.INVALID_ARGUMENT_VALUE,
                              arg.Origin);

                            continue;
                        }
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

                    this.Messages.Critical(
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

        /// <summary>
        /// Gets the set of messages generated while trying to convert the
        /// arguments.
        /// </summary>
        /// <value>The messages generated.</value>
        public IGraphMessageCollection Messages { get; }
    }
}