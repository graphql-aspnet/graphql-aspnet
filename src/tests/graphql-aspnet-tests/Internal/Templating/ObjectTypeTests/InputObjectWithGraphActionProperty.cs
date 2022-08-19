﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Internal.Templating.ObjectTypeTests
{
    using GraphQL.AspNet.Interfaces.Controllers;

    public class InputObjectWithGraphActionProperty
    {
        public int Id { get; set; }

        public IGraphActionResult ActionResult { get; set; }
    }
}