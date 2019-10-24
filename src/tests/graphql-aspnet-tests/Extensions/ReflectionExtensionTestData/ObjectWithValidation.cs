// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Extensions.ReflectionExtensionTestData
{
    using System.ComponentModel.DataAnnotations;

    public class ObjectWithValidation
    {
        [Required]
        public string Property1
        {
            get;
            set;
        }
    }
}