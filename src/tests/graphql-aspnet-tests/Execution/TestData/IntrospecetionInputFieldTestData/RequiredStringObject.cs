// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.IntrospecetionInputFieldTestData
{
    using System.ComponentModel.DataAnnotations;

    public class RequiredStringObject
    {
        [Required]
        public string Property1 { get; set; }
    }
}