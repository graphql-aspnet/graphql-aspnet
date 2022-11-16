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
    public struct StructNotRequiredSetIntObject
    {
        public StructNotRequiredSetIntObject()
        {
            this.Property1 = 5;
        }

        public int Property1 { get; set; }
    }
}