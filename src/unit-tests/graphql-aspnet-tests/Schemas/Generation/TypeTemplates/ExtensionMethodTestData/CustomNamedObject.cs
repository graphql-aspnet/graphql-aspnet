// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Schemas.Generation.TypeTemplates.ExtensionMethodTestData
{
    using GraphQL.AspNet.Attributes;

    [GraphType("Custom_Named_Object")]
    public class CustomNamedObject
    {
        public string FieldOne { get; set; }
    }
}