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
    public class NotRequiredSetStructObject
    {
        public NotRequiredSetStructObject()
        {
            this.Property1 = new PropertyTestStruct()
            {
                TestStructProp1 = 89,
                TestStructProp2 = "default value set 89",
            };
        }

        public PropertyTestStruct Property1 { get; set; }
    }
}