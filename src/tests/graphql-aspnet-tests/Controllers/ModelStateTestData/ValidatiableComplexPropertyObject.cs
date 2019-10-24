// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Controllers.ModelStateTestData
{
    using System.ComponentModel.DataAnnotations;

    public class ValidatiableComplexPropertyObject
    {
        [Required]
        public ValidatiableScalarPropertyObject Child { get; set; }

        [Range(1, 100)]
        public int RangeValue { get; set; }
    }
}