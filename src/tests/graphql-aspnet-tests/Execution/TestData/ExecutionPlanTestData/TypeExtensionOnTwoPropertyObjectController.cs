// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.ExecutionPlanTestData
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;

    public class TypeExtensionOnTwoPropertyObjectController : GraphController
    {
        [QueryRoot]
        public IEnumerable<TwoPropertyObject> RetrieveData()
        {
            var list = new List<TwoPropertyObject>();

            list.Add(new TwoPropertyObject() { Property1 = "Prop1", Property2 = 1 });
            list.Add(new TwoPropertyObject() { Property1 = "Prop2", Property2 = 2 });

            return list;
        }

        [TypeExtension(typeof(TwoPropertyObject), "property3")]
        public string Value2(TwoPropertyObject source)
        {
            return source.Property2.ToString() + "ABC";
        }
    }
}