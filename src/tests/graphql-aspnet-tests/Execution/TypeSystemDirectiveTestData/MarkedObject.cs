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

    [ApplyDirective(typeof(ObjectMarkerDirective))]
    public class MarkedObject
    {
        public string Prop1 { get; set; }
    }
}