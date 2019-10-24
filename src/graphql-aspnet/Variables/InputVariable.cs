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
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Variables;

    /// <summary>
    /// A base input variable defining common items related across all variable types.
    /// </summary>
    public abstract class InputVariable : IInputVariable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InputVariable"/> class.
        /// </summary>
        /// <param name="name">The name of the variable as defined by the user.</param>
        public InputVariable(string name)
        {
            this.Name = Validation.ThrowIfNullWhiteSpaceOrReturn(name, nameof(name));
        }

        /// <summary>
        /// Gets the name of the variable as it was declared in the user's supplied data.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; }
    }
}