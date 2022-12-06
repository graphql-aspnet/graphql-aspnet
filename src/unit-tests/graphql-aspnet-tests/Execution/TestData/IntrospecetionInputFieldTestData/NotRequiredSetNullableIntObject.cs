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
    public class NotRequiredSetNullableIntObject
    {
        public NotRequiredSetNullableIntObject()
        {
            this.Property1 = 567;
        }

        public int? Property1 { get; set; }
    }
}