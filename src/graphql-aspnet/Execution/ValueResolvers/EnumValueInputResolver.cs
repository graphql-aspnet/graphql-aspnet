// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.ValueResolvers
{
    using System;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Resolvables;
    using GraphQL.AspNet.Interfaces.Execution.Variables;

    /// <summary>
    /// A resolver that will convert a source value into a valid <see cref="Enum"/>.
    /// </summary>
    public class EnumValueInputResolver : IInputValueResolver
    {
        private readonly ILeafValueResolver _enumResolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumValueInputResolver" /> class.
        /// </summary>
        /// <param name="resolver">The base resolver for the enum.</param>
        public EnumValueInputResolver(ILeafValueResolver resolver)
        {
            _enumResolver = Validation.ThrowIfNullOrReturn(resolver, nameof(resolver));
        }

        /// <inheritdoc />
        public object Resolve(IResolvableValueItem resolvableItem, IResolvedVariableCollection variableData = null)
        {
            if (resolvableItem is IResolvablePointer pointer)
            {
                IResolvedVariable variable = null;
                var variableFound = variableData?.TryGetValue(pointer.PointsTo, out variable) ?? false;
                if (variableFound)
                    return variable.Value;
            }

            if (resolvableItem is IResolvableValue resolvableValue)
            {
                // enums may be as delimited strings (read from variables collection)
                // or as non-delimited strings (read from a query document)
                var value = GraphQLStrings.UnescapeAndTrimDelimiters(resolvableValue.ResolvableValue, false);
                return _enumResolver.Resolve(value.AsSpan());
            }

            return null;
        }
    }
}