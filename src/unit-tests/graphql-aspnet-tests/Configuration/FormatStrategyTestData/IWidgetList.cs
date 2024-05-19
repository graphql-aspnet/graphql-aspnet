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

    public interface IWidgetList
    {
        [GraphField]
        string TripleListArg(List<IEnumerable<List<string>>> arg1);

        [GraphField]
        string TripleListArgFixed([FromGraphQL(TypeExpression = "[[[Type]]]")] List<IEnumerable<List<string>>> arg1);

        List<IEnumerable<List<string>>> TripleListProp { get; set; }

        [GraphField(TypeExpression = "[[[Type]]]")]
        List<IEnumerable<List<string>>> TripleListPropFixed { get; set; }
    }
}