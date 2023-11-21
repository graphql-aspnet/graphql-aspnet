// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Internal.Templating.ObjectTypeTests
{
    using GraphQL.AspNet.Attributes;

    [GraphType("SomeTypeName")]
    public struct SimpleScalarStructWithTypeOverride
    {
        public int Prop1 { get; set; }
    }
}