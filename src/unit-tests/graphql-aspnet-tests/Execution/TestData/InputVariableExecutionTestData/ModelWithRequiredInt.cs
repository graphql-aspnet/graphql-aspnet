// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.TestData.InputVariableExecutionTestData
{
    using System.ComponentModel.DataAnnotations;

    public class ModelWithRequiredInt
    {
        [Required]
        public int Id { get; set; }
    }
}