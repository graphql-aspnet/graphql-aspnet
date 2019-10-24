// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Variables
{
    /// <summary>
    /// A variable representing one singular value. Be that a number, a string, boolean etc.
    /// </summary>
    public interface IInputSingleValueVariable : IInputVariable
    {
        /// <summary>
        /// Gets the value that was supplied on the request as a string.
        /// </summary>
        /// <value>The value.</value>
        string Value { get; }
    }
}