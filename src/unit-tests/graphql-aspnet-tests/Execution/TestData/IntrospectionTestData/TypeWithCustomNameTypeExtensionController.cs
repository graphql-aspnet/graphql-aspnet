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

    public class TypeWithCustomNameTypeExtensionController : GraphController
    {
        [TypeExtension(typeof(TypeWithCustomName), "fieldThree")]
        public TypeWithCustomName TypeWithCustomNameField3(
            TypeWithCustomName parent)
        {
            return new TypeWithCustomName()
            {
                Field1 = 0,
                Field2 = $"Child_Of_{parent.Field2}",
            };
        }
    }
}