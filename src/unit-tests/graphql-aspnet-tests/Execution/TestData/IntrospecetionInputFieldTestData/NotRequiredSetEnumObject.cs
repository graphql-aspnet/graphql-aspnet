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
    public class NotRequiredSetEnumObject
    {
        public NotRequiredSetEnumObject()
        {
            this.Property1 = InputEnum.Value2;
        }

        public InputEnum Property1 { get; set; }
    }
}