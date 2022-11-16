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
    using System.ComponentModel;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;

    [Description("A Multi Method Directive")]
    public class MultiMethodDirective : GraphDirective
    {
        [DirectiveLocations(DirectiveLocation.FIELD)]
        public IGraphActionResult ForField(int firstArg, TwoPropertyObject secondArg)
        {
            return null;
        }

        [DirectiveLocations(DirectiveLocation.SCALAR)]
        public IGraphActionResult ForTypeSystem(int firstArg, TwoPropertyObject secondArg)
        {
            return null;
        }
    }
}