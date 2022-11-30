// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Internal.Templating.ActionTestData
{
    using System;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;

    [GraphRoute("unionTest")]
    public class UnionTestController : GraphController
    {
        [Query("[action]", "FragmentData", typeof(UnionDataA), typeof(UnionDataB), TypeExpression = TypeExpressions.IsList)]
        public IGraphActionResult TwoTypeUnion()
        {
            throw new NotImplementedException();
        }

        [Query("[action]", typeof(UnionTestProxy))]
        public IGraphActionResult UnionViaProxy()
        {
            throw new NotImplementedException();
        }

        [Query("[action]", "FragmentData", null, null)]
        public IGraphActionResult NoTypesUnion()
        {
            throw new NotImplementedException();
        }

        // two prop object is not castable to the defined query type of IUnionTestDataItem
        [Query("[action]", "FragmentData", typeof(TwoPropertyObject), typeof(UnionDataA))]
        public IGraphActionResult InvalidUnionMember()
        {
            throw new NotImplementedException();
        }

        [Query("[action]", "FragmentData", typeof(int), typeof(UnionDataA))]
        public IGraphActionResult ScalarInUnion()
        {
            throw new NotImplementedException();
        }

        [Query("[action]", "FragmentData", typeof(IGraphActionResult), typeof(UnionDataA))]
        public IGraphActionResult InterfaceDefinedAsUnionMember()
        {
            throw new NotImplementedException();
        }

        [Query("[action]", "FragmentData", typeof(UnionTestProxy), typeof(UnionDataA))]
        public IGraphActionResult UnionProxyAsUnionMember()
        {
            throw new NotImplementedException();
        }
    }
}