// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.TestData.DirectiveProcessorTypeSystemLocationTestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;

    [ApplyDirective(typeof(LocationTestDirective))]
    public class UniontTestObject : GraphUnionProxy
    {
        public UniontTestObject()
            : base(typeof(TwoPropertyObject), typeof(TwoPropertyObjectV2))
        {
        }
    }
}