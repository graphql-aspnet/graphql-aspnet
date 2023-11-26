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

    public class PropertyWithInternalName
    {
        [GraphField(InternalName = "prop_Field_223")]
        public int Prop1 { get; set; }
    }
}