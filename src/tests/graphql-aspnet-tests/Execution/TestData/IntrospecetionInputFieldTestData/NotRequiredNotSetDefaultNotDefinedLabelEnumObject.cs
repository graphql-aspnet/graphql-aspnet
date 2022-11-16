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
    public class NotRequiredNotSetDefaultNotDefinedLabelEnumObject
    {
        // no enum value is set == 0
        public InputEnumNoZeroDefault Property1 { get; set; }
    }
}