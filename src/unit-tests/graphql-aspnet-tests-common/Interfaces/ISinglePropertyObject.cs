// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Common.Interfaces
{
    using GraphQL.AspNet.Attributes;

    /// <summary>
    /// An interface for testing that describes an object with a string property.
    /// </summary>
    [GraphType("TwoPropertyInterface")]
    public interface ISinglePropertyObject
    {
        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>value.</value>
        [GraphField]
        string Property1 { get; }
    }
}