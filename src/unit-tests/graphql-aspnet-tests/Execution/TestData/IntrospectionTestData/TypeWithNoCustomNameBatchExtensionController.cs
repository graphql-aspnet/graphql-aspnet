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
    using System.Collections;
    using System.Collections.Generic;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;

    public class TypeWithNoCustomNameBatchExtensionController : GraphController
    {
        [QueryRoot]
        public TypeWithNoCustomName FetchItem()
        {
            return new TypeWithNoCustomName()
            {
                Field1 = "Data",
            };
        }

        [BatchTypeExtension(typeof(TypeWithNoCustomName), "fieldTwo")]
        public IDictionary<TypeWithNoCustomName, TypeWithCustomName> TypeWithNoCustomNameField2(
            IEnumerable<TypeWithNoCustomName> parents)
        {
            var dic = new Dictionary<TypeWithNoCustomName, TypeWithCustomName>();

            var i = 0;
            foreach (var item in parents)
            {
                dic.Add(item, new TypeWithCustomName()
                {
                    Field1 = i++,
                    Field2 = $"Child_Of_{item.Field1}",
                });
            }

            return dic;
        }
    }
}