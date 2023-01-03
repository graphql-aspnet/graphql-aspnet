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
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.InputArguments;
    using GraphQL.AspNet.Interfaces.Execution.Variables;

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
            messages = messages ?? new GraphMessageCollection();
        }

        /// <summary>
        /// Converts the
        /// </summary>
        /// <param name="variableData">The variable data.</param>
        /// <returns>IExecutionArgumentCollection.</returns>
        public bool TryConvert(IResolvedVariableCollection variableData, out IExecutionArgumentCollection generatedCollection)
        {
            generatedCollection = new ExecutionArgumentCollection(_arguments.Count);
            foreach (var arg in _arguments)
            {
                var resolvedValue = arg.Value.Resolve(variableData);
                generatedCollection.Add(new ExecutionArgument(arg.Argument, resolvedValue));
            }

            return true;
        }

        /// <summary>
        /// Gets the set of messages generated while trying to convert the
        /// arguments.
        /// </summary>
        /// <value>The messages generated.</value>
        public IGraphMessageCollection Messages { get; }
    }
}