// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Configuration
{
    using System;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Configuration.Formatting;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Tests.Common.CommonHelpers;
    using GraphQL.AspNet.Tests.Configuration.FormatStrategyTestData;
    using GraphQL.AspNet.Tests.Framework;
    using NUnit.Framework;
    using NFS = GraphQL.AspNet.Configuration.Formatting.NullabilityFormatStrategy;

    [TestFixture]
    public class GraphSchemaFomatStrategyTests
    {
        // value type, not fixed, object graph type
        [TestCase(NFS.Default, typeof(Widget), "intProp", "Int!")]
        [TestCase(NFS.NonNullStrings, typeof(Widget), "intProp", "Int!")]
        [TestCase(NFS.NonNullLists, typeof(Widget), "intProp", "Int!")]
        [TestCase(NFS.NonNullTemplates, typeof(Widget), "intProp", "Int!")]
        [TestCase(NFS.NonNullReferenceTypes, typeof(Widget), "intProp", "Int!")]

        // value type, fixed as nullable, object graph type
        [TestCase(NFS.Default, typeof(Widget), "fixedIntPropAsNullable", "Int")]
        [TestCase(NFS.NonNullStrings, typeof(Widget), "fixedIntPropAsNullable", "Int")]
        [TestCase(NFS.NonNullLists, typeof(Widget), "fixedIntPropAsNullable", "Int")]
        [TestCase(NFS.NonNullTemplates, typeof(Widget), "fixedIntPropAsNullable", "Int")]
        [TestCase(NFS.NonNullReferenceTypes, typeof(Widget), "fixedIntPropAsNullable", "Int")]

        // value type, fixed as not nullable, object graph type
        [TestCase(NFS.Default, typeof(Widget), "fixedIntPropAsNotNullable", "Int!")]
        [TestCase(NFS.NonNullStrings, typeof(Widget), "fixedIntPropAsNotNullable", "Int!")]
        [TestCase(NFS.NonNullLists, typeof(Widget), "fixedIntPropAsNotNullable", "Int!")]
        [TestCase(NFS.NonNullTemplates, typeof(Widget), "fixedIntPropAsNotNullable", "Int!")]
        [TestCase(NFS.NonNullReferenceTypes, typeof(Widget), "fixedIntPropAsNotNullable", "Int!")]

        // string type, not fixed, object graph type
        [TestCase(NFS.Default, typeof(Widget), "stringProp", "String")]
        [TestCase(NFS.NonNullStrings, typeof(Widget), "stringProp", "String!")]
        [TestCase(NFS.NonNullLists, typeof(Widget), "stringProp", "String")]
        [TestCase(NFS.NonNullTemplates, typeof(Widget), "stringProp", "String")]
        [TestCase(NFS.NonNullReferenceTypes, typeof(Widget), "stringProp", "String")]

        // string, fixed, object graph type
        [TestCase(NFS.Default, typeof(Widget), "fixedStringProp", "String")]
        [TestCase(NFS.NonNullStrings, typeof(Widget), "fixedStringProp", "String")]
        [TestCase(NFS.NonNullLists, typeof(Widget), "fixedStringProp", "String")]
        [TestCase(NFS.NonNullTemplates, typeof(Widget), "fixedStringProp", "String")]
        [TestCase(NFS.NonNullReferenceTypes, typeof(Widget), "fixedStringProp", "String")]

        // object reference, not fixed, object graph type
        [TestCase(NFS.Default, typeof(Widget), "referenceProp", "Widget")]
        [TestCase(NFS.NonNullStrings, typeof(Widget), "referenceProp", "Widget")]
        [TestCase(NFS.NonNullLists, typeof(Widget), "referenceProp", "Widget")]
        [TestCase(NFS.NonNullTemplates, typeof(Widget), "referenceProp", "Widget")]
        [TestCase(NFS.NonNullReferenceTypes, typeof(Widget), "referenceProp", "Widget!")]

        // object reference, fixed, object graph type
        [TestCase(NFS.Default, typeof(Widget), "fixedReferenceProp", "Widget")]
        [TestCase(NFS.NonNullStrings, typeof(Widget), "fixedReferenceProp", "Widget")]
        [TestCase(NFS.NonNullLists, typeof(Widget), "fixedReferenceProp", "Widget")]
        [TestCase(NFS.NonNullTemplates, typeof(Widget), "fixedReferenceProp", "Widget")]
        [TestCase(NFS.NonNullReferenceTypes, typeof(Widget), "fixedReferenceProp", "Widget")]

        // value type, not fixed, interface graph type
        [TestCase(NFS.Default, typeof(IWidgetInterface), "intProp", "Int!")]
        [TestCase(NFS.NonNullStrings, typeof(IWidgetInterface), "intProp", "Int!")]
        [TestCase(NFS.NonNullLists, typeof(IWidgetInterface), "intProp", "Int!")]
        [TestCase(NFS.NonNullTemplates, typeof(IWidgetInterface), "intProp", "Int!")]
        [TestCase(NFS.NonNullReferenceTypes, typeof(IWidgetInterface), "intProp", "Int!")]

        // value type, fixed as nullable, interface graph type
        [TestCase(NFS.Default, typeof(IWidgetInterface), "fixedIntPropAsNullable", "Int")]
        [TestCase(NFS.NonNullStrings, typeof(IWidgetInterface), "fixedIntPropAsNullable", "Int")]
        [TestCase(NFS.NonNullLists, typeof(IWidgetInterface), "fixedIntPropAsNullable", "Int")]
        [TestCase(NFS.NonNullTemplates, typeof(IWidgetInterface), "fixedIntPropAsNullable", "Int")]
        [TestCase(NFS.NonNullReferenceTypes, typeof(IWidgetInterface), "fixedIntPropAsNullable", "Int")]

        // value type, fixed as not nullable, interface graph type
        [TestCase(NFS.Default, typeof(IWidgetInterface), "fixedIntPropAsNotNullable", "Int!")]
        [TestCase(NFS.NonNullStrings, typeof(IWidgetInterface), "fixedIntPropAsNotNullable", "Int!")]
        [TestCase(NFS.NonNullLists, typeof(IWidgetInterface), "fixedIntPropAsNotNullable", "Int!")]
        [TestCase(NFS.NonNullTemplates, typeof(IWidgetInterface), "fixedIntPropAsNotNullable", "Int!")]
        [TestCase(NFS.NonNullReferenceTypes, typeof(IWidgetInterface), "fixedIntPropAsNotNullable", "Int!")]

        // string type, not fixed, interface graph type
        [TestCase(NFS.Default, typeof(IWidgetInterface), "stringProp", "String")]
        [TestCase(NFS.NonNullStrings, typeof(IWidgetInterface), "stringProp", "String!")]
        [TestCase(NFS.NonNullLists, typeof(IWidgetInterface), "stringProp", "String")]
        [TestCase(NFS.NonNullTemplates, typeof(IWidgetInterface), "stringProp", "String")]
        [TestCase(NFS.NonNullReferenceTypes, typeof(IWidgetInterface), "stringProp", "String")]

        // string, fixed, interface graph type
        [TestCase(NFS.Default, typeof(IWidgetInterface), "fixedStringProp", "String")]
        [TestCase(NFS.NonNullStrings, typeof(IWidgetInterface), "fixedStringProp", "String")]
        [TestCase(NFS.NonNullLists, typeof(IWidgetInterface), "fixedStringProp", "String")]
        [TestCase(NFS.NonNullTemplates, typeof(IWidgetInterface), "fixedStringProp", "String")]
        [TestCase(NFS.NonNullReferenceTypes, typeof(IWidgetInterface), "fixedStringProp", "String")]

        // object reference, not fixed, interface graph type
        [TestCase(NFS.Default, typeof(IWidgetInterface), "referenceProp", "IWidgetInterface")]
        [TestCase(NFS.NonNullStrings, typeof(IWidgetInterface), "referenceProp", "IWidgetInterface")]
        [TestCase(NFS.NonNullLists, typeof(IWidgetInterface), "referenceProp", "IWidgetInterface")]
        [TestCase(NFS.NonNullTemplates, typeof(IWidgetInterface), "referenceProp", "IWidgetInterface")]
        [TestCase(NFS.NonNullReferenceTypes, typeof(IWidgetInterface), "referenceProp", "IWidgetInterface!")]

        // object reference, fixed, interface graph type
        [TestCase(NFS.Default, typeof(IWidgetInterface), "fixedReferenceProp", "IWidgetInterface")]
        [TestCase(NFS.NonNullStrings, typeof(IWidgetInterface), "fixedReferenceProp", "IWidgetInterface")]
        [TestCase(NFS.NonNullLists, typeof(IWidgetInterface), "fixedReferenceProp", "IWidgetInterface")]
        [TestCase(NFS.NonNullTemplates, typeof(IWidgetInterface), "fixedReferenceProp", "IWidgetInterface")]
        [TestCase(NFS.NonNullReferenceTypes, typeof(IWidgetInterface), "fixedReferenceProp", "IWidgetInterface")]
        public void Integration_ConcreteObjectTypes_FieldTypeExpressionTests(
            NFS strategy,
            Type concreteType,
            string fieldName,
            string expectedTypeExpression)
        {
            var formatter = new GraphSchemaFormatStrategy();
            formatter.NullabilityStrategy = strategy;

            var server = new TestServerBuilder()
                .AddGraphQL(o =>
                {
                    o.AddType(concreteType);
                    o.DeclarationOptions.SchemaFormatStrategy = formatter;
                })
                .Build();

            var graphType = server.Schema.KnownTypes.FindGraphType(concreteType) as IGraphFieldContainer;
            var field = graphType.Fields[fieldName];

            Assert.AreEqual(expectedTypeExpression, field.TypeExpression.ToString());
        }

        // top level templated item returning a virtual type
        [TestCase(NFS.Default, "Query_Widgets", "path1", "Query_Widgets_Path1")]
        [TestCase(NFS.NonNullTemplates, "Query_Widgets", "path1", "Query_Widgets_Path1!")]
        [TestCase(NFS.NonNullStrings, "Query_Widgets", "path1", "Query_Widgets_Path1")]
        [TestCase(NFS.NonNullLists, "Query_Widgets", "path1", "Query_Widgets_Path1")]
        [TestCase(NFS.NonNullReferenceTypes, "Query_Widgets", "path1", "Query_Widgets_Path1")]

        // nested templated item returning a virtual type
        [TestCase(NFS.Default, "Query_Widgets_Path1", "path2", "Query_Widgets_Path1_Path2")]
        [TestCase(NFS.NonNullTemplates, "Query_Widgets_Path1", "path2", "Query_Widgets_Path1_Path2!")]
        [TestCase(NFS.NonNullStrings, "Query_Widgets_Path1", "path2", "Query_Widgets_Path1_Path2")]
        [TestCase(NFS.NonNullLists, "Query_Widgets_Path1", "path2", "Query_Widgets_Path1_Path2")]
        [TestCase(NFS.NonNullReferenceTypes, "Query_Widgets_Path1", "path2", "Query_Widgets_Path1_Path2")]
        public void Integration_Controllers_FieldTypeExpressionNullabilityTests(
            NFS strategy,
            string graphTypeName,
            string fieldName,
            string expectedTypeExpression)
        {
            var formatter = new GraphSchemaFormatStrategy();
            formatter.NullabilityStrategy = strategy;

            var server = new TestServerBuilder()
                .AddGraphQL(o =>
                {
                    o.AddController<WidgetsController>();
                    o.DeclarationOptions.SchemaFormatStrategy = formatter;
                })
                .Build();

            var graphType = server.Schema.KnownTypes.FindGraphType(graphTypeName) as IGraphFieldContainer;
            var field = graphType.Fields[fieldName];

            Assert.AreEqual(expectedTypeExpression, field.TypeExpression.ToString());
        }

        [TestCase(NFS.Default, "intArgument", "Int!")]
        [TestCase(NFS.NonNullTemplates, "intArgument", "Int!")]
        [TestCase(NFS.NonNullStrings, "intArgument", "Int!")]
        [TestCase(NFS.NonNullReferenceTypes, "intArgument", "Int!")]

        [TestCase(NFS.Default, "intArgumentFixed", "Int!")]
        [TestCase(NFS.NonNullTemplates, "intArgumentFixed", "Int!")]
        [TestCase(NFS.NonNullStrings, "intArgumentFixed", "Int!")]
        [TestCase(NFS.NonNullReferenceTypes, "intArgumentFixed", "Int!")]

        [TestCase(NFS.Default, "stringArgument", "String")]
        [TestCase(NFS.NonNullTemplates, "stringArgument", "String")]
        [TestCase(NFS.NonNullStrings, "stringArgument", "String!")]
        [TestCase(NFS.NonNullReferenceTypes, "stringArgument", "String")]

        [TestCase(NFS.Default, "stringArgumentFixed", "String")]
        [TestCase(NFS.NonNullTemplates, "stringArgumentFixed", "String")]
        [TestCase(NFS.NonNullStrings, "stringArgumentFixed", "String")]
        [TestCase(NFS.NonNullReferenceTypes, "stringArgumentFixed", "String")]

        [TestCase(NFS.Default, "inputObjectArgument", "Input_Widget")]
        [TestCase(NFS.NonNullTemplates, "inputObjectArgument", "Input_Widget")]
        [TestCase(NFS.NonNullStrings, "inputObjectArgument", "Input_Widget")]
        [TestCase(NFS.NonNullReferenceTypes, "inputObjectArgument", "Input_Widget!")]

        [TestCase(NFS.Default, "inputObjectArgumentFixed", "Input_Widget")]
        [TestCase(NFS.NonNullTemplates, "inputObjectArgumentFixed", "Input_Widget")]
        [TestCase(NFS.NonNullStrings, "inputObjectArgumentFixed", "Input_Widget")]
        [TestCase(NFS.NonNullReferenceTypes, "inputObjectArgumentFixed", "Input_Widget")]
        public void Integration_ArgumentTypeExpressionNullabilityTests(
                NFS strategy,
                string fieldName,
                string expectedTypeExpression)
        {
            var formatter = new GraphSchemaFormatStrategy();
            formatter.NullabilityStrategy = strategy;

            var server = new TestServerBuilder()
                .AddGraphQL(o =>
                {
                    o.AddController<WidgetArgumentController>();
                    o.DeclarationOptions.SchemaFormatStrategy = formatter;
                })
                .Build();

            var graphType = server.Schema.KnownTypes.FindGraphType("Query") as IGraphFieldContainer;
            var field = graphType.Fields[fieldName];
            var arg1 = field.Arguments["arg1"];

            Assert.AreEqual(expectedTypeExpression, arg1.TypeExpression.ToString());
        }

        [TestCase(NFS.Default, "tripleListProp", "[[[String]]]")]
        [TestCase(NFS.NonNullStrings, "tripleListProp", "[[[String!]]]")]
        [TestCase(NFS.NonNullReferenceTypes, "tripleListProp", "[[[String]]]")]
        [TestCase(NFS.NonNullLists, "tripleListProp", "[[[String]!]!]!")]
        [TestCase(NFS.NonNullLists | NFS.NonNullStrings,  "tripleListProp", "[[[String!]!]!]!")]

        [TestCase(NFS.Default, "tripleListPropFixed", "[[[String]]]")]
        [TestCase(NFS.NonNullStrings, "tripleListPropFixed", "[[[String]]]")]
        [TestCase(NFS.NonNullLists, "tripleListPropFixed", "[[[String]]]")]
        [TestCase(NFS.NonNullReferenceTypes, "tripleListPropFixed", "[[[String]]]")]
        [TestCase(NFS.NonNullLists | NFS.NonNullStrings, "tripleListPropFixed", "[[[String]]]")]
        public void Integration_Objects_FieldTypeExpressionListNullabilityTests(
            NFS strategy,
            string fieldName,
            string expectedTypeExpression)
        {
            var formatter = new GraphSchemaFormatStrategy();
            formatter.NullabilityStrategy = strategy;

            var server = new TestServerBuilder()
                .AddGraphQL(o =>
                {
                    o.AddType<WidgetList>();
                    o.DeclarationOptions.SchemaFormatStrategy = formatter;
                })
                .Build();

            var graphType = server.Schema.KnownTypes.FindGraphType(typeof(WidgetList)) as IGraphFieldContainer;
            var field = graphType.Fields[fieldName];

            Assert.AreEqual(expectedTypeExpression, field.TypeExpression.ToString());
        }

        [TestCase(NFS.Default, "intArgument", "[Int!]")]
        [TestCase(NFS.NonNullStrings, "intArgument", "[Int!]")]
        [TestCase(NFS.NonNullLists, "intArgument", "[Int!]!")]
        [TestCase(NFS.NonNullLists | NFS.NonNullStrings, "intArgument", "[Int!]!")]

        [TestCase(NFS.Default, "intArgumentFixed", "[Int!]")]
        [TestCase(NFS.NonNullStrings, "intArgumentFixed", "[Int!]")]
        [TestCase(NFS.NonNullLists, "intArgumentFixed", "[Int!]")]
        [TestCase(NFS.NonNullLists | NFS.NonNullStrings, "intArgumentFixed", "[Int!]")]

        [TestCase(NFS.Default, "stringArgument", "[String]")]
        [TestCase(NFS.NonNullStrings, "stringArgument", "[String!]")]
        [TestCase(NFS.NonNullLists, "stringArgument", "[String]!")]
        [TestCase(NFS.NonNullLists | NFS.NonNullStrings, "stringArgument", "[String!]!")]

        [TestCase(NFS.Default, "stringArgumentFixed", "[String]")]
        [TestCase(NFS.NonNullStrings, "stringArgumentFixed", "[String]")]
        [TestCase(NFS.NonNullLists, "stringArgumentFixed", "[String]")]
        [TestCase(NFS.NonNullLists | NFS.NonNullStrings, "stringArgumentFixed", "[String]")]

        [TestCase(NFS.Default, "inputObjectArgument", "[Input_Widget]")]
        [TestCase(NFS.NonNullStrings, "inputObjectArgument", "[Input_Widget]")]
        [TestCase(NFS.NonNullReferenceTypes, "inputObjectArgument", "[Input_Widget!]")]
        [TestCase(NFS.NonNullLists, "inputObjectArgument", "[Input_Widget]!")]
        [TestCase(NFS.NonNullLists | NFS.NonNullStrings, "inputObjectArgument", "[Input_Widget]!")]
        [TestCase(NFS.NonNullLists | NFS.NonNullReferenceTypes, "inputObjectArgument", "[Input_Widget!]!")]

        [TestCase(NFS.Default, "inputObjectArgumentFixed", "[Input_Widget]")]
        [TestCase(NFS.NonNullStrings, "inputObjectArgumentFixed", "[Input_Widget]")]
        [TestCase(NFS.NonNullReferenceTypes, "inputObjectArgumentFixed", "[Input_Widget]")]
        [TestCase(NFS.NonNullLists, "inputObjectArgumentFixed", "[Input_Widget]")]
        [TestCase(NFS.NonNullLists | NFS.NonNullStrings, "inputObjectArgumentFixed", "[Input_Widget]")]
        [TestCase(NFS.NonNullLists | NFS.NonNullReferenceTypes, "inputObjectArgumentFixed", "[Input_Widget]")]
        public void Integration_Controllers_FieldTypeExpressionListNullabilityTests(
            NFS strategy,
            string fieldName,
            string expectedTypeExpression)
        {
            var formatter = new GraphSchemaFormatStrategy();
            formatter.NullabilityStrategy = strategy;

            var server = new TestServerBuilder()
                .AddGraphQL(o =>
                {
                    o.AddController<WidgetListController>();
                    o.DeclarationOptions.SchemaFormatStrategy = formatter;
                })
                .Build();

            var graphType = server.Schema.KnownTypes.FindGraphType("Query") as IGraphFieldContainer;
            var field = graphType.Fields[fieldName];
            var arg1 = field.Arguments["arg1"];

            Assert.AreEqual(expectedTypeExpression, arg1.TypeExpression.ToString());
        }
    }
}