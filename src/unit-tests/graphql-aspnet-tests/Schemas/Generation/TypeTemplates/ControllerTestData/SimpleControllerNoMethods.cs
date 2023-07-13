// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Schemas.Generation.TypeTemplates.ControllerTestData
{
    using System.ComponentModel;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;

    [GraphRoute("simple")]
    [Description("Test Description")]
    public class SimpleControllerNoMethods : GraphController
    {
    }
}