// *************************************************************
//  project:  graphql-aspnet
//  --
//  repo: https://github.com/graphql-aspnet
//  docs: https://graphql-aspnet.github.io
//  --
//  License:  MIT
//  *************************************************************

namespace GraphQL.AspNet.Tests.Execution.TestData.DocumentDescriptionTestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;

    public class DocumentController : GraphController
    {
        [QueryRoot]
        public string DoThing()
        {
            return "success";
        }

        [QueryRoot]
        public TwoPropertyObject DoThingWithObject()
        {
            return new TwoPropertyObject
            {
                Property1 = "success",
                Property2 = 15,
            };
        }

        [QueryRoot]
        public TwoPropertyObject DoThingWithVars(string var1, int var2)
        {
            return new TwoPropertyObject
            {
                Property1 = var1,
                Property2 = var2,
            };
        }
    }
}