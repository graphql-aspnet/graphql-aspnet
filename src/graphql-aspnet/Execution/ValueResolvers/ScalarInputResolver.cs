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
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.PlanGeneration.Resolvables;
    using GraphQL.AspNet.Interfaces.Variables;

    /// <summary>
    /// A resolver that operates in context of a field input value that can generate a qualified .NET object for the
    /// provided scalar data.
    /// </summary>
    public class ScalarInputResolver : BaseValueResolver
    {
        private readonly IScalarValueResolver _scalarResolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScalarInputResolver"/> class.
        /// </summary>
        /// <param name="scalarResolver">The scalar resolver.</param>
        public ScalarInputResolver(IScalarValueResolver scalarResolver)
        {
            _scalarResolver = Validation.ThrowIfNullOrReturn(scalarResolver, nameof(scalarResolver));
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
            var resolver = new ScalarInputResolver(_scalarResolver);
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
            if (resolvableItem is IResolvableValue resolvableValue)
            {
                return _scalarResolver.Resolve(resolvableValue.ResolvableValue);
            }

            return null;
        }
    }
}