// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.TestData.TypeSystemDirectiveTestData
{
    using GraphQL.AspNet.Attributes;

    [ApplyDirective(typeof(InputObjectMarkerDirective))]
    public class MarkedInputObject
    {
        public string Prop1 { get; set; }
    }
}