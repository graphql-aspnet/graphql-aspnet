﻿// *************************************************************
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

    public class NotRequiredNonNullableSetNullableIntObject
    {
        public NotRequiredNonNullableSetNullableIntObject()
        {
            this.Property1 = 3;
        }

        [GraphField(TypeExpression = TypeExpressions.IsNotNull)]
        public int? Property1 { get; set; }
    }
}