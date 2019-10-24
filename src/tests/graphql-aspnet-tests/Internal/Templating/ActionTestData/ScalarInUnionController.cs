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
    using GraphQL.AspNet.Interfaces.Controllers;

    internal class ScalarInUnionController
    {
        [Query("[action]", "FragmentData", typeof(int), typeof(UnionDataA))]
        public IGraphActionResult ScalarInUnion()
        {
            throw new NotImplementedException();
        }
    }
}