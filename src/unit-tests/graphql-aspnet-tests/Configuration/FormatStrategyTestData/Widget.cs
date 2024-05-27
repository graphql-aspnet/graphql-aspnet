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

    public class Widget
    {
        [GraphField]
        public string ArgItem(string arg1)
        {
            return string.Empty;
        }

        [GraphField]
        public string FixedArgItem([FromGraphQL(TypeExpression = "Type")] string arg1)
        {
            return string.Empty;
        }

        public int IntProp { get; set; }

        [GraphField(TypeExpression = "Type")]
        public int FixedIntPropAsNullable { get; set; }

        [GraphField(TypeExpression = "Type!")]
        public int FixedIntPropAsNotNullable { get; set; }

        public string StringProp { get; set; }

        [GraphField(TypeExpression = "Type")]
        public string FixedStringProp { get; set; }

        public Widget ReferenceProp { get; set; }

        [GraphField(TypeExpression = "Type")]
        public Widget FixedReferenceProp { get; set; }
    }
}