// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.CommonHelpers
{
    using GraphQL.AspNet.Attributes;

    [GraphType("TwoPropertyInterface")]
    public interface ITwoPropertyObject
    {
        [GraphField]
        string Property1 { get; }
    }
}