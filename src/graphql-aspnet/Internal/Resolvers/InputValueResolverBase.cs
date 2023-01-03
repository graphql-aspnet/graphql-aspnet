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
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Execution.Source;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Resolvables;
    using GraphQL.AspNet.Interfaces.Execution.Variables;
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// A base class providing common functionality to many input value resolvers.
    /// </summary>
    internal abstract class InputValueResolverBase
    {
        private readonly IDefaultValueSchemaItem _defaultValueProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="InputValueResolverBase"/> class.
        /// </summary>
        /// <param name="defaultValueProvider">A provider that can supply a default value when such value is warranted
        /// during value resolution.</param>
        protected InputValueResolverBase(IDefaultValueSchemaItem defaultValueProvider)
        {
            _defaultValueProvider = defaultValueProvider;
        }

        /// <summary>
        /// Tries to the value via common, universal methods for many scenarios (nullability, variables etc.)
        /// </summary>
        /// <param name="resolvableItem">The resolvable item.</param>
        /// <param name="variableData">The variable data.</param>
        /// <param name="resolvedValue">When successful, this parameter will be populated
        /// with the resolved value.</param>
        /// <returns><c>true</c> if the value was successfully resolved, <c>false</c> otherwise.</returns>
        protected bool TryResolveViaCommonMethods(
            IResolvableValueItem resolvableItem,
            IResolvedVariableCollection variableData,
            out object resolvedValue)
        {
            resolvedValue = null;

            if (resolvableItem is IResolvableNullValue)
                return true;

            if (variableData != null && resolvableItem is IResolvablePointer pointer)
            {
                var variableFound = variableData.TryGetValue(pointer.PointsTo, out var variableValue);
                if (variableFound)
                {
                    resolvedValue = variableValue.Value;

                    // A note exists on rule 5.8.5 (https://spec.graphql.org/October2021/#sec-All-Variable-Usages-are-Allowed.Allowing-optional-variables-when-default-values-exist)
                    // saying that IF a variable is not supplied (meaning default value was used) and IF the default value that was used for the variable is
                    // null and IF the target location defines a default value
                    // the default value of the target location should be used instead
                    if (variableValue.Value == null
                        && variableValue.IsDefaultValue
                        && _defaultValueProvider != null
                        && _defaultValueProvider.HasDefaultValue)
                    {
                        resolvedValue = _defaultValueProvider.DefaultValue;
                        return true;
                    }

                    return true;
                }
            }

            return false;
        }
    }
}