// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.TestData.IntrospecetionInputFieldTestData
{
    public class NotRequiredNotSetInvalidDefaultValueEnumObject
    {
        // the default for this enum is [GraphSkip]
        public InputEnumSkippedFirst Property1 { get; set; }
    }
}