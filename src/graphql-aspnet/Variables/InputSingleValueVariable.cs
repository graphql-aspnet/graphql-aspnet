// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Variables
{
    using System;
    using System.Diagnostics;
    using GraphQL.AspNet.Interfaces.PlanGeneration.Resolvables;
    using GraphQL.AspNet.Interfaces.Variables;

    /// <summary>
    /// A variable representing one singular value. Be that a number, a string, boolean etc.
    /// </summary>
    [DebuggerDisplay("InputValue: {Name}  (Value = {Value})")]
    public class InputSingleValueVariable : InputVariable, IInputSingleValueVariable, IResolvableValue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InputSingleValueVariable" /> class.
        /// </summary>
        /// <param name="name">The name of the variable as defined by the user.</param>
        /// <param name="value">The value of the variable, expressed as a string.</param>
        public InputSingleValueVariable(string name, string value)
            : base(name)
        {
            this.Value = value;
        }

        /// <summary>
        /// Gets the value that was supplied on the request as a string.
        /// </summary>
        /// <value>The value.</value>
        public string Value { get; }

        /// <summary>
        /// Gets the value to be used to resolve to some .NET type.
        /// </summary>
        /// <value>The resolvable value.</value>
        ReadOnlySpan<char> IResolvableValue.ResolvableValue => this.Value.AsSpan();
    }
}