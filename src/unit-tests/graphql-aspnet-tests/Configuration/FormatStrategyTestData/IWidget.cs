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
    using GraphQL.AspNet.Attributes;

    public interface IWidget
    {
        [GraphField]
        string ArgItem(string arg1);

        [GraphField]
        string FixedArgItem([FromGraphQL(TypeExpression = "Type")] string arg1);

        int IntProp { get; set; }

        [GraphField(TypeExpression = "Type")]
        int FixedIntPropAsNullable { get; set; }

        [GraphField(TypeExpression = "Type!")]
        int FixedIntPropAsNotNullable { get; set; }

        string StringProp { get; set; }

        [GraphField(TypeExpression = "Type")]
        string FixedStringProp { get; set; }

        IWidget ReferenceProp { get; set; }

        [GraphField(TypeExpression = "Type")]
        IWidget FixedReferenceProp { get; set; }
    }
}