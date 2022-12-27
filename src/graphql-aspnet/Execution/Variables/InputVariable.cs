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
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Execution.Variables;

    /// <summary>
    /// A base input variable defining items common  across all variable types.
    /// </summary>
    internal abstract class InputVariable : IInputVariable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InputVariable"/> class.
        /// </summary>
        /// <param name="name">The name of the variable as defined by the user.</param>
        public InputVariable(string name)
        {
            this.Name = Validation.ThrowIfNullWhiteSpaceOrReturn(name, nameof(name));
        }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public string Key => this.Name;
    }
}