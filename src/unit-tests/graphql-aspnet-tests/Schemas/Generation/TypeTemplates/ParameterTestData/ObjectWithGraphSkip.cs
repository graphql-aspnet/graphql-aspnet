// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Schemas.Generation.TypeTemplates.ParameterTestData
{
    using GraphQL.AspNet.Attributes;

    [GraphSkip]
    public class ObjectWithGraphSkip
    {
        public int Property1 { get; set; }
    }
}