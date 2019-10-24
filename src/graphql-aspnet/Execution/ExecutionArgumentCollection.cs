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

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionArgumentCollection"/> class.
        /// </summary>
        public ExecutionArgumentCollection()
        {
            _arguments = new Dictionary<string, ExecutionArgument>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionArgumentCollection"/> class.
        /// </summary>
        /// <param name="argumentList">The argument list.</param>
        /// <param name="sourceData">The source data.</param>
        private ExecutionArgumentCollection(IDictionary<string, ExecutionArgument> argumentList, object sourceData)
        {
            _arguments = new Dictionary<string, ExecutionArgument>(argumentList);
            this.SourceData = sourceData;
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

        /// <summary>
        /// Augments the collection with a source data object for a specific field execution and returns
        /// a copy of itself with that data attached.
        /// </summary>
        /// <param name="sourceData">The source data.</param>
        /// <returns>IExecutionArgumentCollection.</returns>
        public IExecutionArgumentCollection WithSourceData(object sourceData)
        {
            return new ExecutionArgumentCollection(_arguments, sourceData);
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
            var paramSet = new List<object>();

            foreach (var argTemplate in graphMethod.Arguments)
            {
                object passedValue = this.ResolveParameterFromArgumentTemplate(argTemplate);
                if (passedValue == null && !argTemplate.TypeExpression.IsNullable)
                {
                    // technically shouldn't be throwable given the validation routines
                    // but captured here as a saftey net for users
                    // doing custom extensions or implementations
                    throw new GraphExecutionException(
                        $"The parameter '{argTemplate.Name}' for field '{graphMethod.Route.Path}' could not be resolved from the query document " +
                        $"or variable collection and no default value was found.");
                }

                paramSet.Add(passedValue);
            }

            return paramSet.ToArray();
        }

        /// <summary>
        /// Attempts to deserialize a parameter value from the graph ql context supplied.
        /// </summary>
        /// <param name="argDefinition">The argument definition.</param>
        /// <returns>System.Object.</returns>
        private object ResolveParameterFromArgumentTemplate(IGraphFieldArgumentTemplate argDefinition)
        {
            if (argDefinition == null)
                return null;

            if (argDefinition.ArgumentModifiers.IsSourceParameter())
                return this.SourceData;

            return this.ContainsKey(argDefinition.DeclaredArgumentName)
                ? this[argDefinition.DeclaredArgumentName].Value
                : argDefinition.DefaultValue;
        }

        /// <summary>
        /// Determines whether the read-only dictionary contains an element that has the specified key.
        /// </summary>
        /// <param name="key">The key to locate.</param>
        /// <returns>true if the read-only dictionary contains an element that has the specified key; otherwise, false.</returns>
        public bool ContainsKey(string key) => _arguments.ContainsKey(key);

        /// <summary>
        /// Gets the value that is associated with the specified key.
        /// </summary>
        /// <param name="key">The key to locate.</param>
        /// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the value parameter. This parameter is passed uninitialized.</param>
        /// <returns>true if the object that implements the <see cref="T:System.Collections.Generic.IReadOnlyDictionary`2"></see> interface contains an element that has the specified key; otherwise, false.</returns>
        public bool TryGetValue(string key, out ExecutionArgument value)
        {
            return _arguments.TryGetValue(key, out value);
        }

        /// <summary>
        /// Gets the <see cref="ExecutionArgument"/> with the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>ExecutionArgument.</returns>
        public ExecutionArgument this[string key] => _arguments[key];

        /// <summary>
        /// Gets an enumerable collection that contains the keys in the read-only dictionary.
        /// </summary>
        /// <value>The keys.</value>
        public IEnumerable<string> Keys => _arguments.Keys;

        /// <summary>
        /// Gets an enumerable collection that contains the values in the read-only dictionary.
        /// </summary>
        /// <value>The values.</value>
        public IEnumerable<ExecutionArgument> Values => _arguments.Values;

        /// <summary>
        /// Gets the number of elements in the collection.
        /// </summary>
        /// <value>The count.</value>
        public int Count => _arguments.Count;

        /// <summary>
        /// Gets the source data, if any, that is supplying values for this execution run.
        /// </summary>
        /// <value>The source data.</value>
        public object SourceData { get; private set; }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<KeyValuePair<string, ExecutionArgument>> GetEnumerator()
        {
            return _arguments.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}