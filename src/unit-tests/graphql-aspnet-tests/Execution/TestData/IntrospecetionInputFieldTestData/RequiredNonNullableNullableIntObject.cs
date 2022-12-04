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
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Schemas.TypeSystem;

    public class RequiredNonNullableNullableIntObject
    {
        [Required]
        [GraphField(TypeExpression = "Type!")]
        public int? Property1 { get; set; }
    }
}