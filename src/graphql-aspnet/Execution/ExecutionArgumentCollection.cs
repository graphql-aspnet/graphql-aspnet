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
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A collection of actionable arguments (.NET objects) that can be directly used to invoke
    /// a method on a resolver.
    /// </summary>
    [DebuggerDisplay("Count = {Count}")]
    internal class ExecutionArgumentCollection : IExecutionArgumentCollection
    {
        private readonly Dictionary<string, ExecutionArgument> _arguments;
        private readonly GraphDirectiveExecutionContext _directiveContext;
        private readonly GraphFieldExecutionContext _fieldContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionArgumentCollection" /> class.
        /// </summary>
        /// <param name="capacity">The initial capacity of the collection, if known.</param>
        public ExecutionArgumentCollection(int? capacity = null)
        {
            _arguments = capacity.HasValue
                ? new Dictionary<string, ExecutionArgument>(capacity.Value)
                : new Dictionary<string, ExecutionArgument>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionArgumentCollection" /> class.
        /// </summary>
        /// <param name="argumentList">The argument list keyed by the argument's name in the graph.</param>
        /// <param name="fieldContext">The field context.</param>
        private ExecutionArgumentCollection(
            IDictionary<string, ExecutionArgument> argumentList,
            GraphFieldExecutionContext fieldContext)
        {
            _arguments = new Dictionary<string, ExecutionArgument>(argumentList);
            _fieldContext = fieldContext;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionArgumentCollection" /> class.
        /// </summary>
        /// <param name="argumentList">The argument list keyed by the argument's name in the graph.</param>
        /// <param name="directiveContext">The directive context.</param>
        private ExecutionArgumentCollection(
            IDictionary<string, ExecutionArgument> argumentList,
            GraphDirectiveExecutionContext directiveContext)
        {
            _arguments = new Dictionary<string, ExecutionArgument>(argumentList);
            _directiveContext = directiveContext;
        }

        /// <inheritdoc />
        public void Add(ExecutionArgument argument)
        {
            Validation.ThrowIfNull(argument, nameof(argument));
            _arguments.Add(argument.Name, argument);
        }

        /// <inheritdoc />
        public IExecutionArgumentCollection ForContext(GraphFieldExecutionContext fieldContext)
        {
            return new ExecutionArgumentCollection(_arguments, fieldContext);
        }

        /// <inheritdoc />
        public IExecutionArgumentCollection ForContext(GraphDirectiveExecutionContext directiveContext)
        {
            return new ExecutionArgumentCollection(_arguments, directiveContext);
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public object[] PrepareArguments(IGraphFieldResolverMetaData resolverMetadata)
        {
            Validation.ThrowIfNull(resolverMetadata, nameof(resolverMetadata));

            var preparedParams = new object[resolverMetadata.Parameters.Count];

            for (var i = 0; i < resolverMetadata.Parameters.Count; i++)
            {
                var parameter = resolverMetadata.Parameters[i];

                object passedValue = this.ResolveParameterValue(parameter);

                var schemaListItem = false;
                if (parameter.ArgumentModifiers.IsPartOfTheSchema())
                {
                    // additional checks and coersion if this the value is
                    // being supplied from a query
                    var graphArgument = _fieldContext?
                        .Request
                        .Field
                        .Arguments
                        .FindArgumentByParameterName(parameter.InternalName);

                    graphArgument = graphArgument ??
                        _directiveContext?
                        .Request
                        .Directive
                        .Arguments
                        .FindArgumentByParameterName(parameter.InternalName);

                    if (graphArgument == null)
                        throw new GraphExecutionException($"Argument '{parameter.InternalName}' was not found on its expected parent.");

                    schemaListItem = graphArgument.TypeExpression.IsListOfItems;
                    if (passedValue == null && !graphArgument.TypeExpression.IsNullable)
                    {
                        // technically shouldn't be possible given the validation routines
                        // but captured here as a saftey net for users
                        // doing custom extensions or implementations
                        throw new GraphExecutionException(
                            $"The parameter '{parameter.InternalName}' for field '{_fieldContext?.Request?.Field?.Route.Path}' could not be resolved from the query document " +
                            "or variable collection and no default value was found.");
                    }
                }

                // ensure compatible list types between the internally
                // tracked data and the target type of the method being invoked
                // e.g. convert List<T> =>  T[]  when needed
                if (parameter.IsList)
                {
                    var listMangler = new ListMangler(parameter.ExpectedType);
                    var result = listMangler.Convert(passedValue);
                    passedValue = result.Data;
                }

                preparedParams[i] = passedValue;
            }

            return preparedParams;
        }

        private object ResolveParameterValue(IGraphFieldResolverParameterMetaData parameterDefinition)
        {
            if (parameterDefinition == null)
                return null;

            if (parameterDefinition.ArgumentModifiers.IsSourceParameter())
                return this.SourceData;

            if (parameterDefinition.ArgumentModifiers.IsCancellationToken())
                return _fieldContext?.CancellationToken ?? default;

            if (parameterDefinition.ArgumentModifiers.IsInjected())
            {
                if (_fieldContext != null)
                    return _fieldContext.ServiceProvider.GetService(parameterDefinition.ExpectedType);

                if (_directiveContext != null)
                    return _directiveContext.ServiceProvider.GetService(parameterDefinition.ExpectedType);

                return null;
            }

            if (this.ContainsKey(parameterDefinition.InternalName))
                return this[parameterDefinition.InternalName].Value;

            return parameterDefinition.DefaultValue;
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
        public object SourceData => _fieldContext?.Request?.Data?.Value;

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