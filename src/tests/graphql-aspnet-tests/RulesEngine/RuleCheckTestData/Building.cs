// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.ValidationRules.RuleCheckTestData
{
    using System.ComponentModel.DataAnnotations;

    public class Building
    {
        public int Id { get; set; }

        public BuildingAddress Address { get; set; }
    }
}