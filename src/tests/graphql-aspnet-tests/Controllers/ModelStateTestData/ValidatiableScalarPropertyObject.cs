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

    public class ValidatiableScalarPropertyObject
    {
        public string NullableProperty { get; set; }

        [Required]
        public string RequiredProperty { get; set; }

        [Required]
        [StringLength(15)]
        public string RequiredLengthProperty { get; set; }

        [Range(5, 15)]
        public int RangeProperty { get; set; }
    }
}