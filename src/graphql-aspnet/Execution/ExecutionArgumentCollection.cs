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
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A collection of actionable arguments (.NET types) that can be directly used to invoke
    /// a method on a resolver.
    /// </summary>
    [DebuggerDisplay("Count = {Count}")]
    public class ExecutionArgumentCollection : IExecutionArgumentCollection
    {
        private readonly Dictionary<string, ExecutionArgument> _arguments;
        private readonly IGraphFieldRequest _fieldRequest;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionArgumentCollection"/> class.
        /// </summary>
        public ExecutionArgumentCollection()
        {
            _arguments = new Dictionary<string, ExecutionArgument>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionArgumentCollection" /> class.
        /// </summary>
        /// <param name="argumentList">The argument list.</param>
        /// <param name="fieldRequest">The field request.</param>
        private ExecutionArgumentCollection(IDictionary<string, ExecutionArgument> argumentList, IGraphFieldRequest fieldRequest)
        {
            _arguments = new Dictionary<string, ExecutionArgument>(argumentList);
            _fieldRequest = fieldRequest;
        }

        /// <summary>
        /// Adds the specified argument.
        /// </summary>
        /// <param name="argument">The argument.</param>
        public void Add(ExecutionArgument argument)
        {
            Validation.ThrowIfNull(argument, nameof(argument));
            _arguments.Add(argument.Name, argument);
        }

        /// <inheritdoc />
        public IExecutionArgumentCollection ForRequest(IGraphFieldRequest fieldRequest)
        {
            return new ExecutionArgumentCollection(_arguments, fieldRequest);
        }

        /// <summary>
        /// Tries the get argument.
        /// </summary>
        /// <typeparam name="TType">The type of the t type.</typeparam>
        /// <param name="argumentName">Name of the argument.</param>
        /// <param name="argumentValue">The argument value.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool TryGetArgument<TType>(string argumentName, out TType argumentValue)
        {
            argumentValue = default;
            if (!this.ContainsKey(argumentName))
                return false;

            try
            {
                argumentValue = (TType)this[argumentName].Value;
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Prepares this colleciton of arguments for use in the provided method invocation generating
        /// a set of values that can be used to invoke the method.
        /// </summary>
        /// <param name="graphMethod">The graph method.</param>
        /// <returns>System.Object[].</returns>
        public object[] PrepareArguments(IGraphMethod graphMethod)
        {
            var preparedParams = new List<object>();
            var paramInfos = graphMethod.Method.GetParameters();

            for (var i = 0; i < graphMethod.Arguments.Count; i++)
            {
                var argTemplate = graphMethod.Arguments[i];
                object passedValue = this.ResolveParameterFromArgumentTemplate(argTemplate);
                if (passedValue == null && !argTemplate.TypeExpression.IsNullable)
                {
                    // technically shouldn't be throwable given the validation routines
                    // but captured here as a saftey net for users
                    // doing custom extensions or implementations
                    throw new GraphExecutionException(
                        $"The parameter '{argTemplate.Name}' for field '{graphMethod.Route.Path}' could not be resolved from the query document " +
                        "or variable collection and no default value was found.");
                }

                // ensure compatible list types between the internally
                // tracked data and the target type of the method being invoked
                // i.e. convert List<T> =>  T[]  when needed
                if (argTemplate.TypeExpression.IsListOfItems)
                {
                    var listMangler = new ListMangler(paramInfos[i].ParameterType);
                    var result = listMangler.Convert(passedValue);
                    passedValue = result.Data;
                }

                preparedParams.Add(passedValue);
            }

            return preparedParams.ToArray();
        }

        /// <summary>
        /// Attempts to deserialize a parameter value from the graph ql context supplied.
        /// </summary>
        /// <param name="argDefinition">The argument definition.</param>
        /// <returns>System.Object.</returns>
        private object ResolveParameterFromArgumentTemplate(IGraphArgumentTemplate argDefinition)
        {
            if (argDefinition == null)
                return null;

            if (argDefinition.ArgumentModifiers.IsSourceParameter())
                return this.SourceData;

            if (argDefinition.ArgumentModifiers.IsCancellationToken())
                return _fieldRequest?.OperationRequest?.CancelToken ?? default;

            return this.ContainsKey(argDefinition.DeclaredArgumentName)
                ? this[argDefinition.DeclaredArgumentName].Value
                : argDefinition.DefaultValue;
        }

        /// <inheritdoc />
        public bool ContainsKey(string key) => _arguments.ContainsKey(key);

        /// <inheritdoc />
        public bool TryGetValue(string key, out ExecutionArgument value)
        {
            return _arguments.TryGetValue(key, out value);
        }

        /// <inheritdoc />
        public ExecutionArgument this[string key] => _arguments[key];

        /// <inheritdoc />
        public IEnumerable<string> Keys => _arguments.Keys;

        /// <inheritdoc />
        public IEnumerable<ExecutionArgument> Values => _arguments.Values;

        /// <inheritdoc />
        public int Count => _arguments.Count;

        /// <inheritdoc />
        public object SourceData => _fieldRequest?.Data.Value;

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<string, ExecutionArgument>> GetEnumerator()
        {
            return _arguments.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}