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
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A collection of actionable arguments (.NET objects) that can be directly used to invoke
    /// a method on a resolver.
    /// </summary>
    [DebuggerDisplay("Count = {Count}")]
    internal class ExecutionArgumentCollection : IExecutionArgumentCollection
    {
        private readonly Dictionary<string, ExecutionArgument> _suppliedArgumentDataValues;
        private readonly SchemaItemResolutionContext _resolutionContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionArgumentCollection" /> class.
        /// </summary>
        /// <param name="capacity">The initial capacity of the collection, if known.</param>
        public ExecutionArgumentCollection(int? capacity = null)
        {
            _suppliedArgumentDataValues = capacity.HasValue
                ? new Dictionary<string, ExecutionArgument>(capacity.Value)
                : new Dictionary<string, ExecutionArgument>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionArgumentCollection" /> class.
        /// </summary>
        /// <param name="argumentList">The argument list keyed by the argument's name in the graph.</param>
        /// <param name="resolutionContext">The resolution context.</param>
        private ExecutionArgumentCollection(
            IDictionary<string, ExecutionArgument> argumentList,
            SchemaItemResolutionContext resolutionContext)
        {
            _suppliedArgumentDataValues = new Dictionary<string, ExecutionArgument>(argumentList);
            _resolutionContext = resolutionContext;
        }

        /// <inheritdoc />
        public void Add(ExecutionArgument argument)
        {
            Validation.ThrowIfNull(argument, nameof(argument));
            _suppliedArgumentDataValues.Add(argument.Name, argument);
        }

        /// <inheritdoc />
        public IExecutionArgumentCollection ForContext(SchemaItemResolutionContext resolutionContext)
        {
            return new ExecutionArgumentCollection(_suppliedArgumentDataValues, resolutionContext);
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

        private object ResolveParameterValue(IGraphFieldResolverParameterMetaData paramDef)
        {
            Validation.ThrowIfNull(paramDef, nameof(paramDef));

            if (paramDef.ArgumentModifiers.IsSourceParameter())
                return _resolutionContext?.SourceData;

            if (paramDef.ArgumentModifiers.IsCancellationToken())
                return _resolutionContext?.CancellationToken;

            if (paramDef.ArgumentModifiers.IsResolverContext())
            {
                if (_resolutionContext != null && Validation.IsCastable(_resolutionContext.GetType(), paramDef.ExpectedType))
                    return _resolutionContext;

                return null;
            }

            // if there an argument supplied on the query for this parameter, use that
            if (this.TryGetValue(paramDef.InternalName, out var arg))
                return arg.Value;

            // if the parameter is part of the graph, use the related argument's default value
            if (paramDef.ArgumentModifiers.CouldBePartOfTheSchema())
            {
                // additional checks and coersion if this the value is
                // being supplied from a query
                var graphArgument = _resolutionContext?
                    .SchemaDefinedArguments?
                    .FindArgumentByParameterName(paramDef.InternalName);

                if (graphArgument != null)
                {
                    if (graphArgument.HasDefaultValue)
                        return graphArgument.DefaultValue;

                    if (graphArgument.TypeExpression.IsNullable)
                        return null;

                    // When an argument is found on the schema
                    //   and no value was supplied on the query
                    //   and that argument has no default value defined
                    //   and that argument is not allowed to be null
                    //   then error out
                    //
                    // This situation is technically not possible given the validation routines in place at runtime
                    // but captured here as a saftey net for users
                    // doing custom extensions or implementations
                    // this prevents resolver execution with indeterminate or unexpected data
                    var path = _resolutionContext?.Route.Path;
                    throw new GraphExecutionException(
                        $"The parameter '{paramDef.InternalName}' for schema item '{path}' could not be resolved from the query document " +
                        "or variable collection and no default value was found.");
                }
            }

            // its not a formal argument in the schema, try and resolve from DI container
            object serviceResolvedValue = _resolutionContext?.ServiceProvider?.GetService(paramDef.ExpectedType);

            // the service was found in the DI container!! *happy*
            if (serviceResolvedValue != null)
                return serviceResolvedValue;

            // it wasn't found but the developer declared a fall back. *thankful*
            if (paramDef.HasDefaultValue)
                return paramDef.DefaultValue;

            var schemaItem = _resolutionContext?.Route.Path
                ?? paramDef.InternalFullName;

            // error unable to resolve correctly. *womp womp*
            throw new GraphExecutionException(
                   $"The parameter '{paramDef.InternalName}' targeting '{schemaItem}' was expected to be resolved from a " +
                   $"service provider but a suitable instance could not be obtained from the current invocation context " +
                   $"and no default value was declared.");
        }

        /// <inheritdoc />
        public bool ContainsKey(string key) => _suppliedArgumentDataValues.ContainsKey(key);

        /// <inheritdoc />
        public bool TryGetValue(string key, out ExecutionArgument value)
        {
            return _suppliedArgumentDataValues.TryGetValue(key, out value);
        }

        /// <inheritdoc />
        public ExecutionArgument this[string key] => _suppliedArgumentDataValues[key];

        /// <inheritdoc />
        public IEnumerable<string> Keys => _suppliedArgumentDataValues.Keys;

        /// <inheritdoc />
        public IEnumerable<ExecutionArgument> Values => _suppliedArgumentDataValues.Values;

        /// <inheritdoc />
        public int Count => _suppliedArgumentDataValues.Count;

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<string, ExecutionArgument>> GetEnumerator()
        {
            return _suppliedArgumentDataValues.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}