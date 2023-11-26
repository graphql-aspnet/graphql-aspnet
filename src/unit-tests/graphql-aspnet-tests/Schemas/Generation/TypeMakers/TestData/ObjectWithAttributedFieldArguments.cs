// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Schemas.Generation.TypeMakers.TestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Tests.Common.CommonHelpers;
    using GraphQL.AspNet.Tests.Common.Interfaces;
    using Microsoft.AspNetCore.Mvc;

    public class ObjectWithAttributedFieldArguments
    {
        [GraphField]
        public int FieldWithExplicitServiceArg([FromServices] int arg1)
        {
            return 0;
        }

        [GraphField]
        public int FieldWithImplicitArgThatShouldBeServiceInjected(ISinglePropertyObject arg1)
        {
            return 0;
        }

        [GraphField]
        public int FieldWithImplicitArgThatShouldBeInGraph(TwoPropertyObject arg1)
        {
            return 0;
        }
    }
}