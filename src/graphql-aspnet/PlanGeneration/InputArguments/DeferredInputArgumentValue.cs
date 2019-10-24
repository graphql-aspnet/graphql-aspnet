// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.PlanGeneration.InputArguments
{
    using System.Diagnostics;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.PlanGeneration;
    using GraphQL.AspNet.Interfaces.Variables;
    using GraphQL.AspNet.PlanGeneration.Document.Parts.QueryInputValues;

    /// <summary>
    /// A represenetation of an argument to an execution context that has yet to be fully realized
    /// and will need to make use of runtime data (variables provided by a user request) to finalize its
    /// value so that it can provide it to the context.
    /// </summary>
    [DebuggerDisplay("{_coreValue.OwnerArgument.Name}")]
    public class DeferredInputArgumentValue : IInputArgumentValue
    {
        private readonly QueryInputValue _coreValue;
        private readonly IInputValueResolver _resolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeferredInputArgumentValue"/> class.
        /// </summary>
        /// <param name="coreValue">The core value.</param>
        /// <param name="valueResolver">The resolver to use when rendering out a variable value for this
        /// argument.</param>
        public DeferredInputArgumentValue(
            QueryInputValue coreValue,
            IInputValueResolver valueResolver)
        {
            _coreValue = Validation.ThrowIfNullOrReturn(coreValue, nameof(coreValue));
            _resolver = Validation.ThrowIfNullOrReturn(valueResolver, nameof(valueResolver));
        }

        /// <summary>
        /// Resolves the final value of this input argument using the supplied variable for any replacements necessary.
        /// The variable collection can be null and is ignored if this argument does not require use of it.
        /// </summary>
        /// <param name="variableData">The variable data.</param>
        /// <returns>System.Object.</returns>
        public object Resolve(IResolvedVariableCollection variableData)
        {
            return _resolver.WithVariables(variableData).Resolve(_coreValue);
        }
    }
}