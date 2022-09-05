﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Web.CancelTokenTestData
{
    using System.Threading;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;

    public class MathController : GraphController
    {
        public CancellationToken ReceivedToken { get; private set; }

        [QueryRoot]
        public int Add(int a, int b, CancellationToken cancelToken)
        {
            this.ReceivedToken = cancelToken;
            return a + b;
        }
    }
}