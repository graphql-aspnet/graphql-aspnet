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

    /// <summary>
    /// An internal input argument data resolver that converts parted document parts, derived from
    /// <see cref="IResolvableValueItem" />, into valid .NET objects of an appropriate type.
    /// </summary>
    public interface IInputValueResolver
    {
        /// <summary>
        /// Resolves the specified query input value to the .NET object rendered by this resolver.
        /// </summary>
        /// <param name="resolvableItem">The resolvable item.</param>
        /// <param name="variableData">An optiona collection of supplied variable data that
        /// can be used to resolve variable references contained within the <paramref name="resolvableItem"/>.</param>
        /// <returns>System.Object.</returns>
        object Resolve(IResolvableValueItem resolvableItem, IResolvedVariableCollection variableData = null);
    }
}