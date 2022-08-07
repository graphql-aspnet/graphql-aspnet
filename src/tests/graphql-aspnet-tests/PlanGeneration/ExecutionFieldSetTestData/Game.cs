// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.PlanGeneration.ExecutionFieldSetTestData
{
    using System.Collections.Generic;

    public class Game
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public GameType GameType { get; set; }

        public float HoursOfPlay { get; set; }

        public IEnumerable<Game> RelatedGames { get; set; }
    }
}