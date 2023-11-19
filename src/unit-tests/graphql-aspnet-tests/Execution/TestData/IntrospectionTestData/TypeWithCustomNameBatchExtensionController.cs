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

    public class TypeWithCustomNameBatchExtensionController : GraphController
    {
        [BatchTypeExtension(typeof(TypeWithCustomName), "fieldThree")]
        public IDictionary<TypeWithCustomName, IEnumerable<TypeWithCustomName>> TypeWithCustomNameField3(
            IEnumerable<TypeWithCustomName> parents)
        {
            var dic = new Dictionary<TypeWithCustomName, IEnumerable<TypeWithCustomName>>();
            var i = 0;
            foreach (var item in parents)
            {
                dic.Add(item, new List<TypeWithCustomName>()
                {
                    new TypeWithCustomName()
                    {
                        Field1 = i++,
                        Field2 = $"Child_Of_{item.Field2}",
                    },
                    new TypeWithCustomName()
                    {
                        Field1 = i++,
                        Field2 = $"Child_Of_{item.Field2}",
                    },
                });
            }

            return dic;
        }
    }
}