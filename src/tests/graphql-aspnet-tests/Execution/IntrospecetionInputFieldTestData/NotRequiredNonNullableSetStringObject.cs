// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.IntrospecetionInputFieldTestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Schemas.TypeSystem;

    public class NotRequiredNonNullableSetStringObject
    {
        public NotRequiredNonNullableSetStringObject()
        {
            this.Property1 = "default string value";
        }

        [GraphField(TypeExpression = TypeExpressions.IsNotNull)]
        public string Property1 { get; set; }
    }
}