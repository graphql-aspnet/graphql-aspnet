// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Execution.TypeSystemDirectiveTestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Schemas;

    [ApplyDirective(typeof(SchemaMarkerDirective))]
    public class MarkedSchema : GraphSchema
    {
    }
}