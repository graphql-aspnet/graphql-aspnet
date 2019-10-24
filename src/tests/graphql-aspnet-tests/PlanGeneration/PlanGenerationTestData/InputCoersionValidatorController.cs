// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.PlanGeneration.PlanGenerationTestData
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;

    [GraphRoot]
    public class InputCoersionValidatorController : GraphController
    {
        [Query]
        public TestUser SingleScalarIntInput(IEnumerable<int> arg1)
        {
            return null;
        }
    }
}