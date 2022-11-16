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

    public class NotRequiredSetClassObject
    {
        public NotRequiredSetClassObject()
        {
            this.Property1 = new TwoPropertyObject()
            {
                Property1 = "prop 1 default",
                Property2 = 38,
            };
        }

        public TwoPropertyObject Property1 { get; set; }
    }
}