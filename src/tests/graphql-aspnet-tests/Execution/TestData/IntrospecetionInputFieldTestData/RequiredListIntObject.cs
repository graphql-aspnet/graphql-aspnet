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
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class RequiredListIntObject
    {
        [Required]
        public List<int> Property1 { get; set; }
    }
}