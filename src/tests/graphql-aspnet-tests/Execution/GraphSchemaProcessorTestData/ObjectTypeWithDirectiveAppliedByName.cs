// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Execution.GraphSchemaProcessorTestData
{
    using GraphQL.AspNet.Attributes;

    [ApplyDirective("MyDirective")]
    public class ObjectTypeWithDirectiveAppliedByName
    {
        public int Property1 { get; set; }
    }
}