// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.QueryPlans.PlanGenerationTestData
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Schemas.TypeSystem;

    [GraphRoute("fragTester")]
    public class FragmentProcessingController : GraphController
    {
        [Query(
            "makeHybridData",
            "FragmentData",
            typeof(FragmentDataA),
            typeof(FragmentDataB),
            TypeExpression = "[Type]")]
        public IGraphActionResult GenerateHybridDataSet()
        {
            var list = new List<IFragmentDataItem>();
            list.Add(new FragmentDataA()
            {
                Property1 = "0",
                Property2 = "0",
            });
            list.Add(new FragmentDataA()
            {
                Property1 = "1",
                Property2 = "1",
            });
            list.Add(new FragmentDataB()
            {
                Property1 = "0",
                Property2 = "0",
            });
            list.Add(new FragmentDataB()
            {
                Property1 = "1",
                Property2 = "1",
            });

            return this.Ok(list);
        }

        [Query(
            "sourceDataInheritance",
            "FragmentData3",
            typeof(FragmentDataA),
            typeof(FragmentDataB),
            typeof(FragmentDataC),
            TypeExpression = "[Type]")]
        public IGraphActionResult SourceDataWithInheritedObjects()
        {
            var list = new List<IFragmentDataItem>();
            list.Add(new FragmentDataA()
            {
                Property1 = "0",
                Property2 = "0",
            });
            list.Add(new FragmentDataB()
            {
                Property1 = "0",
                Property2 = "0",
            });
            list.Add(new FragmentDataC()
            {
                Property1 = "0",
                Property2 = "0",
            });

            return this.Ok(list);
        }
    }
}