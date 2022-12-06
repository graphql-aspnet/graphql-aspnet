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

    public class RequiredDoubleObject
    {
        [Required]
        public double Property1 { get; set; }
    }
}