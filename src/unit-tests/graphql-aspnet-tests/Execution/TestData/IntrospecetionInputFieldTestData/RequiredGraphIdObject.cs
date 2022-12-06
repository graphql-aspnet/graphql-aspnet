// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.TestData.IntrospecetionInputFieldTestData
{
    using System.ComponentModel.DataAnnotations;

    public class RequiredGraphIdObject
    {
        [Required]
        public GraphId Property1 { get; set; }
    }
}