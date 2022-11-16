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
    using System;
    using System.ComponentModel.DataAnnotations;

    public class RequiredDateTimeOffsetObject
    {
        [Required]
        public DateTimeOffset Property1 { get; set; }
    }
}