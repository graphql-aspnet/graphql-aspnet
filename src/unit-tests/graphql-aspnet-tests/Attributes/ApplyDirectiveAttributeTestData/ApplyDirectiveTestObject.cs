// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Attributes.ApplyDirectiveAttributeTestData
{
    using GraphQL.AspNet.Tests.Common.CommonHelpers;

    [InheritedApplyDirective(typeof(TwoPropertyObject), "arg1")]
    internal class ApplyDirectiveTestObject
    {
    }
}