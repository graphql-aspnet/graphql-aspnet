// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Schemas.Generation.TypeTemplates.PropertyTestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Tests.Schemas.Generation.TypeTemplates.DirectiveTestData;

    public class PropertyClassWithDirective
    {
        [ApplyDirective(typeof(DirectiveWithArgs), 55, "property arg")]
        public int Prop1 { get; set; }
    }
}