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

    public class MarkedInputFieldObject
    {
        [ApplyDirective(typeof(InputObjectFieldMarkerDirective))]
        public string Prop1 { get; set; }
    }
}