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

    [GraphType(InternalName = "InputObjectInternalName")]
    public class InputTestObjectWithInternalName
    {
        [GraphField(InternalName = "Prop1InternalName")]
        public int Prop1 { get; set; }
    }
}