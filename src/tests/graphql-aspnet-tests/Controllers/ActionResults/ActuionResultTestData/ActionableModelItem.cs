// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Controllers.ActionResults.ActuionResultTestData
{
    using System.ComponentModel.DataAnnotations;

    public class ActionableModelItem
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Id { get; set; }
    }
}