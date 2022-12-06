// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Engine.DefaultDocumentGeneratorTestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;

    public class BakeryController : GraphController
    {
        [QueryRoot]
        public Donut RetrieveDonut(int id)
        {
            return null;
        }
    }
}