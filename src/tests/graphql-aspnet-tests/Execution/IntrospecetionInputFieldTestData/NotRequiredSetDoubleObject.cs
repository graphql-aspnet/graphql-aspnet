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
    public class NotRequiredSetDoubleObject
    {
        public NotRequiredSetDoubleObject()
        {
            this.Property1 = 1.2345;
        }

        public double Property1 { get; set; }
    }
}