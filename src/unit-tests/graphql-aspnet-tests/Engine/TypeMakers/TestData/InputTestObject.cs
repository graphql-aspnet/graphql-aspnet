// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Engine.TypeMakers.TestData
{
    using System.ComponentModel.DataAnnotations;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Common.CommonHelpers;

    public class InputTestObject
    {
        [ApplyDirective("DirectiveForNotRequiredValueTypeField")]
        public int NotRequiredValueTypeField { get; set; }

        [Required]
        public int RequiredValueTypeField { get; set; }

        [Required]
        public GraphId GraphIdRequired { get; set; }

        public GraphId GraphIdNotRequired { get; set; }

        public GraphId? GraphIdNullable { get; set; }

        public TwoPropertyObject NotRequiredReferenceTypeField { get; set; }

        [Required]
        public TwoPropertyObject RequiredReferenceTypeField { get; set; }

        [Required]
        [GraphField(TypeExpression = "Type!")]
        public TwoPropertyObject RequiredReferenceExplicitNonNullTypeField { get; set; }
    }
}