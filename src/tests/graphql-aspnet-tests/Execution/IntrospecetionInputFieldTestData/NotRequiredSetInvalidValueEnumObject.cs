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
    public class NotRequiredSetInvalidValueEnumObject
    {
        public NotRequiredSetInvalidValueEnumObject()
        {
            // value3 is not in the schema
            this.Property1 = InputEnum.Value3;
        }

        public InputEnum Property1 { get; set; }
    }
}