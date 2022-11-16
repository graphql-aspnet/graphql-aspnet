// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Execution.TestData.GraphSchemaProcessorTestData
{
    using GraphQL.AspNet.Attributes;

    [ApplyDirective(typeof(ParameterizedDirectiveForProcessor), 5, "string 6")]
    public class ObjectTypeWithDirectiveArguments
    {
        public int Property1 { get; set; }
    }
}