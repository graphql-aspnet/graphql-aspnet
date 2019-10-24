// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Execution
{
    using GraphQL.AspNet.Interfaces.PlanGeneration.Resolvables;
    using GraphQL.AspNet.Interfaces.Variables;
    using GraphQL.AspNet.PlanGeneration.Document.Parts.QueryInputValues;

    /// <summary>
    /// A resolver capable of taking in a document part derived from <see cref="QueryInputValue" />
    /// and generate a .NET object of an appropriate type.
    /// </summary>
    public interface IInputValueResolver
    {
        /// <summary>
        /// Creates a copy of this value resolver injecting it with a specific collection
        /// of variables that can be used during resoltuion. Any previously wrapped variable sets
        /// should be discarded.
        /// </summary>
        /// <param name="variableData">The variable data.</param>
        /// <returns>IInputValueResolver.</returns>
        IInputValueResolver WithVariables(IResolvedVariableCollection variableData);

        /// <summary>
        /// Resolves the specified query input value to the .NET object rendered by this resolver.
        /// </summary>
        /// <param name="resolvableItem">The resolvable item.</param>
        /// <returns>System.Object.</returns>
        object Resolve(IResolvableItem resolvableItem);
    }
}