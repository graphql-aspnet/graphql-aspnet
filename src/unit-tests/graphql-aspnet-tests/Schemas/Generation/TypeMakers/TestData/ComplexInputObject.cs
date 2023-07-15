// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Schemas.Generation.TypeMakers.TestData
{
    using GraphQL.AspNet.Attributes;

    public class ComplexInputObject
    {
        [GraphField]
        public string Prop1 { get; set; }

        [GraphField]
        public OneMarkedProperty Prop2 { get; set; }
    }
}