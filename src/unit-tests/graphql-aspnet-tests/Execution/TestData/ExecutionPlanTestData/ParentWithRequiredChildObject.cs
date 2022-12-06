// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.TestData.ExecutionPlanTestData
{
    using System.ComponentModel.DataAnnotations;

    public class ParentWithRequiredChildObject
    {
        public string Property1 { get; set; }

        [Required]
        public NullableChildObject Child { get; set; }
    }
}