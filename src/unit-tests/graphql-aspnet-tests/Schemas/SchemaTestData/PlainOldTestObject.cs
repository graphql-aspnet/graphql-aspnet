// *************************************************************
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

    public class PlainOldTestObject
    {
        [GraphField]
        public string Prop1 { get; set; }

        public string Prop2 { get; set; }
    }
}