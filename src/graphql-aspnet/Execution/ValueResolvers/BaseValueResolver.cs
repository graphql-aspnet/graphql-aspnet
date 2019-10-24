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
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.PlanGeneration.Resolvables;
    using GraphQL.AspNet.Interfaces.Variables;
    using GraphQL.AspNet.PlanGeneration.Document.Parts.QueryInputValues;

    /// <summary>
    /// A base class for logic common to all value resolvers.
    /// </summary>
    public abstract class BaseValueResolver : IInputValueResolver
    {
        /// <summary>
        /// Creates a copy of this value resolver injecting it with a specific collection
        /// of variables that can be used during resoltuion. Any previously wrapped variable sets
        /// should be discarded.
        /// </summary>
        /// <param name="variableData">The variable data.</param>
        /// <returns>IInputValueResolver.</returns>
        public abstract IInputValueResolver WithVariables(IResolvedVariableCollection variableData);

        /// <summary>
        /// Resolves the provided query input value to the .NET object rendered by this resolver.
        /// This input value is garunteed to not be a variable reference.
        /// </summary>
        /// <param name="resolvableItem">The resolvable item.</param>
        /// <returns>System.Object.</returns>
        protected abstract object ResolveFromItem(IResolvableItem resolvableItem);

        /// <summary>
        /// Resolves the specified query input value to the .NET object rendered by this resolver.
        /// </summary>
        /// <param name="resolvableItem">The resolvable item.</param>
        /// <returns>object.</returns>
        public object Resolve(IResolvableItem resolvableItem)
        {
            if (resolvableItem is IResolvablePointer pointer)
            {
                IResolvedVariable variable = null;
                var variableFound = this.VariableCollection?.TryGetValue(pointer.PointsTo, out variable) ?? false;
                if (variableFound)
                    return variable.Value;

                resolvableItem = pointer.DefaultItem;
            }

            return this.ResolveFromItem(resolvableItem);
        }

        /// <summary>
        /// Attempts to find the variable using the input value and the user supplied variable data collection.
        /// </summary>
        /// <param name="variableCollection">The variable collection.</param>
        /// <param name="inputValue">The input value representing a value to be resolved from a query document.</param>
        /// <param name="variable">The variable that was found in the collection.</param>
        /// <returns><c>true</c> if the varible was found, <c>false</c> otherwise.</returns>
        public bool TryGetVariable(IInputVariableCollection variableCollection, QueryInputValue inputValue, out IInputVariable variable)
        {
            variable = null;
            if (variableCollection == null || !(inputValue is QueryVariableReferenceInputValue varReference))
                return false;

            return variableCollection.TryGetVariable(varReference.VariableName, out variable);
        }

        /// <summary>
        /// Gets or sets the variable collection in scope on this resolver, may be null.
        /// </summary>
        /// <value>The variable collection.</value>
        protected IResolvedVariableCollection VariableCollection { get; set; }
    }
}