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
    using GraphQL.AspNet.Interfaces.PlanGeneration;
    using GraphQL.AspNet.Interfaces.Variables;

    /// <summary>
    /// A represenetation of an argument to a field execution context that has already been fully realized
    /// and will return a constant value.
    /// </summary>
    [DebuggerDisplay("{Name}")]
    public class ResolvedInputArgumentValue : IInputArgumentValue
    {
        private readonly object _preResolvedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResolvedInputArgumentValue" /> class.
        /// </summary>
        /// <param name="argumentName">Name of the argument.</param>
        /// <param name="preResolvedValue">The pre resolved value that needs no further alterations to be served.</param>
        public ResolvedInputArgumentValue(string argumentName, object preResolvedValue)
        {
            this.Name = Validation.ThrowIfNullWhiteSpaceOrReturn(argumentName, nameof(argumentName));
            _preResolvedValue = preResolvedValue;
        }

        /// <summary>
        /// Resolves the final value of this input argument using the supplied variable for any replacements necessary.
        /// The variable collection can be null and is ignored if this argument does not require use of it.
        /// </summary>
        /// <param name="variableData">The variable data.</param>
        /// <returns>System.Object.</returns>
        public object Resolve(IResolvedVariableCollection variableData)
        {
            return _preResolvedValue;
        }

        /// <summary>
        /// Gets the name of the value as it was defined in a query document.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; }
    }
}