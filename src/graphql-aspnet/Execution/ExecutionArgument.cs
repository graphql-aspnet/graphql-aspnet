// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution
{
    using System.Diagnostics;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// An encapsulated object destined to be used as an input parameter to a method or controller action
    /// in an attempt to resolve a field request.
    /// </summary>
    [DebuggerDisplay("{Name}")]
    public class ExecutionArgument
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionArgument" /> class.
        /// </summary>
        /// <param name="argument">The argument.</param>
        /// <param name="value">The value.</param>
        public ExecutionArgument(IGraphFieldArgument argument, object value)
        {
            this.Argument = Validation.ThrowIfNullOrReturn(argument, nameof(argument));
            this.Value = value;
        }

        /// <summary>
        /// Gets the name of this argument as its defined in the internal method signature (the parameter name).
        /// </summary>
        /// <value>The name.</value>
        public string Name => this.Argument.ParameterName;

        /// <summary>
        /// Gets the argument reference this execution value represents.
        /// </summary>
        /// <value>The argument.</value>
        public IGraphFieldArgument Argument { get; }

        /// <summary>
        /// Gets the physical value being used for this argument.
        /// </summary>
        /// <value>The value.</value>
        public object Value { get; }
    }
}