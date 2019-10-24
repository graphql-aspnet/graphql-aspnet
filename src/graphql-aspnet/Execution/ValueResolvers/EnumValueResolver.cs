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
    using GraphQL.AspNet.Interfaces.PlanGeneration.Resolvables;
    using GraphQL.AspNet.Interfaces.Variables;

    /// <summary>
    /// A resolver that will convert a source value into a valid <see cref="Enum"/>.
    /// </summary>
    public class EnumValueResolver : BaseValueResolver
    {
        private readonly Type _enumType;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumValueResolver"/> class.
        /// </summary>
        /// <param name="enumType">Type of the enum.</param>
        public EnumValueResolver(Type enumType)
        {
            _enumType = Validation.ThrowIfNullOrNotEnumOrReturn(enumType, nameof(enumType));
        }

        /// <summary>
        /// Creates a copy of this value resolver injecting it with a specific collection
        /// of variables that can be used during resoltuion. Any previously wrapped variable sets
        /// should be discarded.
        /// </summary>
        /// <param name="variableData">The variable data.</param>
        /// <returns>IInputValueResolver.</returns>
        public override IInputValueResolver WithVariables(IResolvedVariableCollection variableData)
        {
            var resolver = new EnumValueResolver(_enumType);
            resolver.VariableCollection = variableData;
            return resolver;
        }

        /// <summary>
        /// Resolves the provided query input value to the .NET object rendered by this resolver.
        /// This input value is garunteed to not be a variable reference.
        /// </summary>
        /// <param name="resolvableItem">The resolvable item.</param>
        /// <returns>System.Object.</returns>
        protected override object ResolveFromItem(IResolvableItem resolvableItem)
        {
            string value = null;
            try
            {
                if (resolvableItem is IResolvableValue resolvableValue)
                {
                    // enums may be as delimited strings (read from variables collection)
                    // or as non-delimited strings (read from a query document)
                    value = GraphQLStrings.UnescapeAndTrimDelimiters(resolvableValue.ResolvableValue, false);
                    return Enum.Parse(_enumType, value, true);
                }
            }
            catch
            {
                // ignored
            }

            return null;
        }
    }
}