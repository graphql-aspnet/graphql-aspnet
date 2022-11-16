// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Execution.RulesEngine.RuleCheckTestData
{
    using System.ComponentModel.DataAnnotations;

    public class BuildingAddress
    {
        [Required]
        public int Id { get; set; }

        public string Name { get; set; }
    }
}