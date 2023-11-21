// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.TestData.IntrospectionTestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;

    public class TypeWithNoCustomNameTypeExtensionController : GraphController
    {
        [QueryRoot]
        public TypeWithNoCustomName FetchItem()
        {
            return new TypeWithNoCustomName()
            {
                Field1 = "Data",
            };
        }

        [TypeExtension(typeof(TypeWithNoCustomName), "fieldTwo")]
        public TypeWithCustomName TypeWithNoCustomNameField2(
            TypeWithNoCustomName parent)
        {
            return new TypeWithCustomName()
            {
                Field1 = 1,
                Field2 = $"Child_Of_{parent.Field1}",
            };
        }
    }
}