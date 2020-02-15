// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Framework.CommonHelpers
{
    using GraphQL.AspNet.Attributes;

    /// <summary>
    /// An interface for testing that describes an object with a string property.
    /// </summary>
    [GraphType("TwoPropertyInterface")]
    public interface ITwoPropertyObject
    {
        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>value.</value>
        [GraphField]
        string Property1 { get; }
    }
}