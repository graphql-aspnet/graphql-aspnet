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