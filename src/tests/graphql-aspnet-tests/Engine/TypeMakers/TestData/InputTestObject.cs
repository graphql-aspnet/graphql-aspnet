// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Defaults.TypeMakers.TestData
{
    using System.ComponentModel.DataAnnotations;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;

    public class InputTestObject
    {
        [ApplyDirective("DirectiveForNotRequiredValueTypeField")]
        public int NotRequiredValueTypeField { get; set; }

        [Required]
        public int RequiredValueTypeField { get; set; }

        [Required]
        public GraphId GraphIdRequired { get; set; }

        public GraphId GraphIdNotRequired { get; set; }

        [GraphField(TypeExpression = TypeExpressions.IsNotNull)]
        public GraphId GraphIdNonNullable { get; set; }

        public TwoPropertyObject NotRequiredReferenceTypeField { get; set; }

        [Required]
        public TwoPropertyObject RequiredReferenceTypeField { get; set; }

        [Required]
        [GraphField(TypeExpression = TypeExpressions.IsNotNull)]
        public TwoPropertyObject RequiredReferenceExplicitNonNullTypeField { get; set; }
    }
}