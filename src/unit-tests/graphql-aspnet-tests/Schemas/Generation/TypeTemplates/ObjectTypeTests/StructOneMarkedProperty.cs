// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Schemas.Generation.TypeTemplates.ObjectTypeTests
{
    using GraphQL.AspNet.Attributes;

    public struct StructOneMarkedProperty
    {
        public string Prop1 { get; set; }

        [GraphField]
        public string Prop2 { get; set; }
    }
}