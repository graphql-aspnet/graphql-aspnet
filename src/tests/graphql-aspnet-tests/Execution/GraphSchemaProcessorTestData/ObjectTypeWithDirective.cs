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

    [ApplyDirective(typeof(SimpleTestDirectiveForProcessor))]
    public class ObjectTypeWithDirective
    {
        public int Property1 { get; set; }
    }
}