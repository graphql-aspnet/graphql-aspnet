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
    public class TestUserController : GraphController
    {
        [Query]
        public List<TestUser> RetrieveUsers()
        {
            return null;
        }

        [Query]
        public TestUser RetrieveUser(int id)
        {
            return null;
        }

        [Query]
        public List<TestUser> ComplexUserMethod(TestUser arg1, int arg2)
        {
            return null;
        }

        [Query]
        public TestUser NestedInputObjectMethod(TestUserHome arg1, int arg2)
        {
            return null;
        }

        [Query]
        public TestUser MultiScalarInput(IEnumerable<int> arg1, int arg2)
        {
            return null;
        }

        [Query]
        public TestUser MultiScalarOfScalarInput(IEnumerable<IEnumerable<int>> arg1)
        {
            return null;
        }

        [Query]
        public TestUser MultiUserInput(IEnumerable<TestUser> arg1, int arg2)
        {
            return null;
        }

        [Query]
        public TestUserHome MultiLevelOutput()
        {
            return null;
        }
    }
}