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
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;

    public class ArrayPropertyObject
    {
        public string Prop1 { get; set; }

        public TwoPropertyObject[] Prop2 { get; set; }
    }
}