// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Response
{
    using System.Diagnostics;
    using GraphQL.AspNet.Interfaces.Response;

    /// <summary>
    /// An instance of a resolved scalar or enum included in a response package.
    /// </summary>
    [DebuggerDisplay("Value = {Value}")]
    public class ResponseSingleValue : IResponseSingleValue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResponseSingleValue"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public ResponseSingleValue(object value)
        {
            this.Value = value;
        }

        /// <summary>
        /// Gets the generated value for this single item instance.
        /// </summary>
        /// <value>The value.</value>
        public object Value { get; }
    }
}