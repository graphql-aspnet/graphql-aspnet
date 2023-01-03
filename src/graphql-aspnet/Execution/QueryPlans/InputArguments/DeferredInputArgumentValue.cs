// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.QueryPlans.InputArguments
{
    using System.Diagnostics;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.InputArguments;
    using GraphQL.AspNet.Interfaces.Execution.Variables;

    /// <summary>
    /// A represenetation of a value to an argument or input field that has yet to be fully realized
    /// and will need to make use of runtime data (variables provided by on user request) to finalize its
    /// value so that it can provide a value to a resolution context.
    /// </summary>
    [DebuggerDisplay("{_coreValue.OwnerArgument.Name}")]
    internal class DeferredInputArgumentValue : IInputValue
    {
        private readonly ISuppliedValueDocumentPart _coreValue;
        private readonly IInputValueResolver _resolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeferredInputArgumentValue" /> class.
        /// </summary>
        /// <param name="coreValue">The core value.</param>
        /// <param name="valueResolver">The resolver to use when rendering out a variable value for this
        /// argument.</param>
        public DeferredInputArgumentValue(
            ISuppliedValueDocumentPart coreValue,
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
            return _resolver.Resolve(_coreValue, variableData);
        }
    }
}