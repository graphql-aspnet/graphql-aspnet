// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.IntrospecetionInputFieldTestData
{
    using GraphQL.AspNet.Attributes;

    public enum InputEnumSkippedFirst
    {
        [GraphSkip]
        Value1 = 0,
        Value2 = 1,
        Value3 = 2,
    }
}