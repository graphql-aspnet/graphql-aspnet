// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.PlanGeneration.DocumentConstructionTestData
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;

    public class BakeryController : GraphController
    {
        [Query]
        public List<Bagel> CreateBagels(int totalBagels)
        {
            return null;
        }

        [Query]
        public Donut RetrieveDonut(int id)
        {
            return null;
        }

        [QueryRoot]
        public Donut inlineFragTest(int id)
        {
            return null;
        }

        [QueryRoot]
        public Donut MultiScalarArguments(int id, float count)
        {
            return null;
        }

        [Query]
        public Donut RetrieveDonutByFlavor(DonutFlavor flavor)
        {
            return null;
        }

        [Query]
        public Donut RetrieveDonutByNullableId(int? id)
        {
            return null;
        }

        [QueryRoot]
        public bool AddDonut(Donut newDonut)
        {
            return false;
        }

        [Query]
        public IEnumerable<Bagel> RetrieveBagelsById(List<int> bagelNumbers)
        {
            return null;
        }

        [Query]
        public bool AddBagelsInOrder(IEnumerable<Bagel> bagels, List<int> orderToAdd)
        {
            return false;
        }

        [QueryRoot]
        public IEnumerable<Donut> RetrieveAllDonuts()
        {
            return null;
        }

        [QueryRoot]
        public int ComplexScenario(IEnumerable<ComplexInput> items)
        {
            return 0;
        }

        [QueryRoot]
        public int NestedList(List<IEnumerable<int[]>> items)
        {
            return 0;
        }
    }
}