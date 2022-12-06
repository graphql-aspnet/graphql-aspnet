// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.QueryPlans.ExecutionFieldSetTestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Interfaces.Controllers;

    public class GameController : GraphController
    {
        [QueryRoot]
        public Game RetrieveGame(int id)
        {
            return null;
        }

        [QueryRoot("retrieveAnyGame", "GameOrGame2", typeof(Game), typeof(Game2))]
        public IGraphActionResult RetrieveAnyGame()
        {
            return null;
        }
    }
}