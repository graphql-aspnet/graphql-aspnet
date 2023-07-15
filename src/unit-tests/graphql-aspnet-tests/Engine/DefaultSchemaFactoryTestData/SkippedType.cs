// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Engine.DefaultSchemaFactoryTestData
{
    using GraphQL.AspNet.Attributes;

    [GraphSkip]
    [GraphType(PreventAutoInclusion = true)]
    public class SkippedType
    {
        public int Prop1 { get; set; }
    }
}