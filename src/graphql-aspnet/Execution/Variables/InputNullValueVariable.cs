// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.Variables
{
    using System.Diagnostics;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Resolvables;
    using GraphQL.AspNet.Interfaces.Execution.Variables;

    /// <summary>
    /// An item representing a variable value as being <c>null</c>.
    /// </summary>
    [DebuggerDisplay("InputValue: {Name}  (Value = null)")]
    internal sealed class InputNullValueVariable : InputVariable, IInputSingleValueVariable, IResolvableNullValue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InputNullValueVariable" /> class.
        /// </summary>
        /// <param name="name">The name of the variable as defined by the user.</param>
        public InputNullValueVariable(string name)
            : base(name)
        {
            // this input variable intentionally does not implement IResolvableValue
            // this allows it to act as a variable that never indicates a value and is always treated
            // as null
        }

        /// <inheritdoc />
        public string Value => null;
    }
}