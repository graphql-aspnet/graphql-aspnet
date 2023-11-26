// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.TestData.RuntimeTypeExtensionTestData
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Tests.Common.CommonHelpers;

    public class RuntimeFieldTypeExtensionTestsController : GraphController
    {
        [QueryRoot]
        public TwoPropertyObject RetrieveObject()
        {
            return new TwoPropertyObject()
            {
                Property1 = "Prop1",
                Property2 = 101,
            };
        }

        [QueryRoot]
        public TwoPropertyStruct RetrieveStruct()
        {
            return new TwoPropertyStruct()
            {
                Property1 = "Prop1",
                Property2 = 101,
            };
        }

        [QueryRoot]
        public IEnumerable<TwoPropertyObject> RetrieveObjects()
        {
            var list = new List<TwoPropertyObject>();
            list.Add(new TwoPropertyObject()
            {
                Property1 = "Prop1A",
                Property2 = 101,
            });
            list.Add(new TwoPropertyObject()
            {
                Property1 = "Prop1B",
                Property2 = 102,
            });
            list.Add(new TwoPropertyObject()
            {
                Property1 = "Prop1C",
                Property2 = 103,
            });

            return list;
        }

        [QueryRoot]
        public IEnumerable<TwoPropertyStruct> RetrieveStructs()
        {
            var list = new List<TwoPropertyStruct>();
            list.Add(new TwoPropertyStruct()
            {
                Property1 = "Prop1A",
                Property2 = 101,
            });
            list.Add(new TwoPropertyStruct()
            {
                Property1 = "Prop1B",
                Property2 = 102,
            });
            list.Add(new TwoPropertyStruct()
            {
                Property1 = "Prop1C",
                Property2 = 103,
            });

            return list;
        }
    }
}