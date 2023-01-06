// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Internal.Resolvers
{
    using System;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Execution.Source;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Resolvables;
    using GraphQL.AspNet.Interfaces.Execution.Variables;
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// A resolver that will convert a source value into a valid <see cref="Enum"/>.
    /// </summary>
    internal class EnumInputValueResolver : InputValueResolverBase, IInputValueResolver
    {
        private readonly ILeafValueResolver _enumResolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumInputValueResolver" /> class.
        /// </summary>
        /// <param name="resolver">The root resolver for the enum.</param>
        /// <param name="defaultValueProvider">A provider that can supply a default value when such value is warranted
        /// during value resolution.</param>
        public EnumInputValueResolver(ILeafValueResolver resolver, IDefaultValueSchemaItem defaultValueProvider = null)
            : base(defaultValueProvider)
        {
            _enumResolver = Validation.ThrowIfNullOrReturn(resolver, nameof(resolver));
        }

        /// <inheritdoc />
        public object Resolve(IResolvableValueItem resolvableItem, IResolvedVariableCollection variableData = null)
        {
            if (this.TryResolveViaCommonMethods(resolvableItem, variableData, out var resolvedValue))
                return resolvedValue;

            if (resolvableItem is IResolvableValue resolvableValue)
            {
                // enums may be as delimited strings (read from variables collection)
                // or as non-delimited strings (read from a query document)
                var value = GraphQLStrings.UnescapeAndTrimDelimiters(resolvableValue.ResolvableValue, false);
                return _enumResolver.Resolve(value.AsSpan());
            }

            throw new UnresolvedValueException("Unresolvable enum data value.");
        }
    }
}