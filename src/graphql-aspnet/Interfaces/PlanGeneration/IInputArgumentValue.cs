// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.PlanGeneration
{
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Interfaces.Variables;

    /// <summary>
    /// A represenetation of an input value, resolved from a request, marrying the value provided on the request
    /// and the internal <see cref="IGraphFieldArgument"/> that it represents in a field of target schema.
    /// </summary>
    public interface IInputArgumentValue
    {
        /// <summary>
        /// Resolves the final value of this input argument using the supplied variable for any replacements necessary.
        /// The variable collection can be null and is ignored if this argument does not require use of it.
        /// </summary>
        /// <param name="variableData">The variable data.</param>
        /// <returns>System.Object.</returns>
        object Resolve(IResolvedVariableCollection variableData);
    }
}