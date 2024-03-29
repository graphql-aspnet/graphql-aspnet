﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Schemas.SchemaTestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;

    public class OneFieldMapController : GraphController
    {
        [Subscription]
        public TwoPropertyObject TestActionMethod(TwoPropertyObject sourceData, string arg1, int arg2)
        {
            return sourceData;
        }
    }
}