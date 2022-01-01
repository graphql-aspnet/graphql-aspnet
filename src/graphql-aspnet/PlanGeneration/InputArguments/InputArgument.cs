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
    using System;
    using System.Diagnostics;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.PlanGeneration;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// A single argument to a field as its represented in a parsed query document. This represents
    /// the marrying of the field's defined argument in the schema and the run time context of the argument value.
    /// </summary>
    [Serializable]
    [DebuggerDisplay("{Name}")]
    public class InputArgument
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InputArgument" /> class.
        /// </summary>
        /// <param name="argument">The field argument defined in a schema.</param>
        /// <param name="value">The value representing this field argument as its defined in a query document.</param>
        public InputArgument(IGraphArgument argument, IInputArgumentValue value)
        {
            this.Argument = Validation.ThrowIfNullOrReturn(argument, nameof(argument));
            this.Value = Validation.ThrowIfNullOrReturn(value, nameof(value));
        }

        /// <summary>
        /// Gets the formal name of argument on the field, in the schema.
        /// </summary>
        /// <value>The name.</value>
        public string Name => this.Argument.Name;

        /// <summary>
        /// Gets the assigned value of the input argument.
        /// </summary>
        /// <value>The value.</value>
        public IInputArgumentValue Value { get; }

        /// <summary>
        /// Gets or sets the underlying field that represents
        /// this argument on the target schema.
        /// </summary>
        /// <value>The argument.</value>
        public IGraphArgument Argument { get; set; }
    }
}