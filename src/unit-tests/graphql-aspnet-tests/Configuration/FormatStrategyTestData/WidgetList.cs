// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Configuration.FormatStrategyTestData
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Attributes;

    public class WidgetList
    {
        public List<IEnumerable<List<string>>> TripleListProp { get; set; }

        [GraphField(TypeExpression = "[[[Type]]]")]
        public List<IEnumerable<List<string>>> TripleListPropFixed { get; set; }
    }
}