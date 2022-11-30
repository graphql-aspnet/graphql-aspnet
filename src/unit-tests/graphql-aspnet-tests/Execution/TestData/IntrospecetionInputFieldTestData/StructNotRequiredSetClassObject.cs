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
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;

    public struct StructNotRequiredSetClassObject
    {
        public StructNotRequiredSetClassObject()
        {
            this.Property1 = new TwoPropertyObject()
            {
                Property1 = "struct set prop value",
                Property2 = 99,
            };
        }

        public TwoPropertyObject Property1 { get; set; }
    }
}