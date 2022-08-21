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
    public class NotRequiredSetStringObject
    {
        public NotRequiredSetStringObject()
        {
            this.Property1 = "prop 1 default set";
        }

        public string Property1 { get; set; }
    }
}