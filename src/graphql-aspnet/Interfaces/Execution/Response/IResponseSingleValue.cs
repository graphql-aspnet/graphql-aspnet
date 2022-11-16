// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Interfaces.Response
{
    /// <summary>
    /// An interface representing a single scalar or enum value generated
    /// in a graphql response.
    /// </summary>
    internal interface IResponseSingleValue : IResponseItem
    {
        /// <summary>
        /// Gets the generated value for this single item instance.
        /// </summary>
        /// <value>The value.</value>
        object Value { get; }
    }
}