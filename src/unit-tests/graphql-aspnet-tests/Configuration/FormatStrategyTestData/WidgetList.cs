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
        [GraphField]
        public string TripleListArg(List<IEnumerable<List<string>>> arg1)
        {
            return string.Empty;
        }

        [GraphField]
        public string TripleListArgFixed([FromGraphQL(TypeExpression = "[[[Type]]]")] List<IEnumerable<List<string>>> arg1)
        {
            return string.Empty;
        }

        [GraphField(TypeExpression = "[[[Type]]]")]
        public List<IEnumerable<List<string>>> TripleListPropFixed { get; set; }

        public List<IEnumerable<List<string>>> TripleListProp { get; set; }
    }
}