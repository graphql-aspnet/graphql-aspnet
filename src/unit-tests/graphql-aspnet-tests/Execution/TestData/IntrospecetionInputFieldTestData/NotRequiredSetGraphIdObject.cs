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
    public class NotRequiredSetGraphIdObject
    {
        public NotRequiredSetGraphIdObject()
        {
            this.Property1 = new GraphId("abc");
        }

        public GraphId Property1 { get; set; }
    }
}