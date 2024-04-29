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
    using GraphQL.AspNet.Configuration.Formatting;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Configuration.FormatStrategyTestData;
    using GraphQL.AspNet.Tests.Framework;
    using NUnit.Framework;
    using NFS = GraphQL.AspNet.Configuration.Formatting.NullabilityFormatStrategy;

    [TestFixture]
    public class GraphSchemaFomatStrategyTests
    {
        // value type, not fixed, object graph type
        [TestCase(NFS.None, typeof(Widget), "intProp", "Int!")]
        [TestCase(NFS.NonNullStrings, typeof(Widget), "intProp", "Int!")]
        [TestCase(NFS.NonNullLists, typeof(Widget), "intProp", "Int!")]
        [TestCase(NFS.NonNullTemplates, typeof(Widget), "intProp", "Int!")]
        [TestCase(NFS.NonNullReferenceTypes, typeof(Widget), "intProp", "Int!")]

        // value type, fixed as nullable, object graph type
        [TestCase(NFS.None, typeof(Widget), "fixedIntPropAsNullable", "Int")]
        [TestCase(NFS.NonNullStrings, typeof(Widget), "fixedIntPropAsNullable", "Int")]
        [TestCase(NFS.NonNullLists, typeof(Widget), "fixedIntPropAsNullable", "Int")]
        [TestCase(NFS.NonNullTemplates, typeof(Widget), "fixedIntPropAsNullable", "Int")]
        [TestCase(NFS.NonNullReferenceTypes, typeof(Widget), "fixedIntPropAsNullable", "Int")]

        // value type, fixed as not nullable, object graph type
        [TestCase(NFS.None, typeof(Widget), "fixedIntPropAsNotNullable", "Int!")]
        [TestCase(NFS.NonNullStrings, typeof(Widget), "fixedIntPropAsNotNullable", "Int!")]
        [TestCase(NFS.NonNullLists, typeof(Widget), "fixedIntPropAsNotNullable", "Int!")]
        [TestCase(NFS.NonNullTemplates, typeof(Widget), "fixedIntPropAsNotNullable", "Int!")]
        [TestCase(NFS.NonNullReferenceTypes, typeof(Widget), "fixedIntPropAsNotNullable", "Int!")]

        // string type, not fixed, object graph type
        [TestCase(NFS.None, typeof(Widget), "stringProp", "String")]
        [TestCase(NFS.NonNullStrings, typeof(Widget), "stringProp", "String!")]
        [TestCase(NFS.NonNullInputStrings, typeof(Widget), "stringProp", "String")]
        [TestCase(NFS.NonNullOutputStrings, typeof(Widget), "stringProp", "String!")]
        [TestCase(NFS.NonNullLists, typeof(Widget), "stringProp", "String")]
        [TestCase(NFS.NonNullTemplates, typeof(Widget), "stringProp", "String")]
        [TestCase(NFS.NonNullReferenceTypes, typeof(Widget), "stringProp", "String")]

        // string, fixed, object graph type
        [TestCase(NFS.None, typeof(Widget), "fixedStringProp", "String")]
        [TestCase(NFS.NonNullStrings, typeof(Widget), "fixedStringProp", "String")]
        [TestCase(NFS.NonNullInputStrings, typeof(Widget), "fixedStringProp", "String")]
        [TestCase(NFS.NonNullOutputStrings, typeof(Widget), "fixedStringProp", "String")]
        [TestCase(NFS.NonNullLists, typeof(Widget), "fixedStringProp", "String")]
        [TestCase(NFS.NonNullTemplates, typeof(Widget), "fixedStringProp", "String")]
        [TestCase(NFS.NonNullReferenceTypes, typeof(Widget), "fixedStringProp", "String")]

        // object reference, not fixed, object graph type
        [TestCase(NFS.None, typeof(Widget), "referenceProp", "Widget")]
        [TestCase(NFS.NonNullStrings, typeof(Widget), "referenceProp", "Widget")]
        [TestCase(NFS.NonNullLists, typeof(Widget), "referenceProp", "Widget")]
        [TestCase(NFS.NonNullTemplates, typeof(Widget), "referenceProp", "Widget")]
        [TestCase(NFS.NonNullReferenceTypes, typeof(Widget), "referenceProp", "Widget!")]

        // object reference, fixed, object graph type
        [TestCase(NFS.None, typeof(Widget), "fixedReferenceProp", "Widget")]
        [TestCase(NFS.NonNullStrings, typeof(Widget), "fixedReferenceProp", "Widget")]
        [TestCase(NFS.NonNullLists, typeof(Widget), "fixedReferenceProp", "Widget")]
        [TestCase(NFS.NonNullTemplates, typeof(Widget), "fixedReferenceProp", "Widget")]
        [TestCase(NFS.NonNullReferenceTypes, typeof(Widget), "fixedReferenceProp", "Widget")]

        // value type, not fixed, interface graph type
        [TestCase(NFS.None, typeof(IWidgetInterface), "intProp", "Int!")]
        [TestCase(NFS.NonNullStrings, typeof(IWidgetInterface), "intProp", "Int!")]
        [TestCase(NFS.NonNullLists, typeof(IWidgetInterface), "intProp", "Int!")]
        [TestCase(NFS.NonNullTemplates, typeof(IWidgetInterface), "intProp", "Int!")]
        [TestCase(NFS.NonNullReferenceTypes, typeof(IWidgetInterface), "intProp", "Int!")]

        // value type, fixed as nullable, interface graph type
        [TestCase(NFS.None, typeof(IWidgetInterface), "fixedIntPropAsNullable", "Int")]
        [TestCase(NFS.NonNullStrings, typeof(IWidgetInterface), "fixedIntPropAsNullable", "Int")]
        [TestCase(NFS.NonNullLists, typeof(IWidgetInterface), "fixedIntPropAsNullable", "Int")]
        [TestCase(NFS.NonNullTemplates, typeof(IWidgetInterface), "fixedIntPropAsNullable", "Int")]
        [TestCase(NFS.NonNullReferenceTypes, typeof(IWidgetInterface), "fixedIntPropAsNullable", "Int")]

        // value type, fixed as not nullable, interface graph type
        [TestCase(NFS.None, typeof(IWidgetInterface), "fixedIntPropAsNotNullable", "Int!")]
        [TestCase(NFS.NonNullStrings, typeof(IWidgetInterface), "fixedIntPropAsNotNullable", "Int!")]
        [TestCase(NFS.NonNullLists, typeof(IWidgetInterface), "fixedIntPropAsNotNullable", "Int!")]
        [TestCase(NFS.NonNullTemplates, typeof(IWidgetInterface), "fixedIntPropAsNotNullable", "Int!")]
        [TestCase(NFS.NonNullReferenceTypes, typeof(IWidgetInterface), "fixedIntPropAsNotNullable", "Int!")]

        // string type, not fixed, interface graph type
        [TestCase(NFS.None, typeof(IWidgetInterface), "stringProp", "String")]
        [TestCase(NFS.NonNullStrings, typeof(IWidgetInterface), "stringProp", "String!")]
        [TestCase(NFS.NonNullInputStrings, typeof(IWidgetInterface), "stringProp", "String")]
        [TestCase(NFS.NonNullOutputStrings, typeof(IWidgetInterface), "stringProp", "String!")]
        [TestCase(NFS.NonNullLists, typeof(IWidgetInterface), "stringProp", "String")]
        [TestCase(NFS.NonNullTemplates, typeof(IWidgetInterface), "stringProp", "String")]
        [TestCase(NFS.NonNullReferenceTypes, typeof(IWidgetInterface), "stringProp", "String")]

        // string, fixed, interface graph type
        [TestCase(NFS.None, typeof(IWidgetInterface), "fixedStringProp", "String")]
        [TestCase(NFS.NonNullStrings, typeof(IWidgetInterface), "fixedStringProp", "String")]
        [TestCase(NFS.NonNullInputStrings, typeof(IWidgetInterface), "fixedStringProp", "String")]
        [TestCase(NFS.NonNullOutputStrings, typeof(IWidgetInterface), "fixedStringProp", "String")]
        [TestCase(NFS.NonNullLists, typeof(IWidgetInterface), "fixedStringProp", "String")]
        [TestCase(NFS.NonNullTemplates, typeof(IWidgetInterface), "fixedStringProp", "String")]
        [TestCase(NFS.NonNullReferenceTypes, typeof(IWidgetInterface), "fixedStringProp", "String")]

        // object reference, not fixed, interface graph type
        [TestCase(NFS.None, typeof(IWidgetInterface), "referenceProp", "IWidgetInterface")]
        [TestCase(NFS.NonNullStrings, typeof(IWidgetInterface), "referenceProp", "IWidgetInterface")]
        [TestCase(NFS.NonNullLists, typeof(IWidgetInterface), "referenceProp", "IWidgetInterface")]
        [TestCase(NFS.NonNullTemplates, typeof(IWidgetInterface), "referenceProp", "IWidgetInterface")]
        [TestCase(NFS.NonNullReferenceTypes, typeof(IWidgetInterface), "referenceProp", "IWidgetInterface!")]

        // object reference, fixed, interface graph type
        [TestCase(NFS.None, typeof(IWidgetInterface), "fixedReferenceProp", "IWidgetInterface")]
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
        [TestCase(NFS.None, "Query_Widgets", "path1", "Query_Widgets_Path1")]
        [TestCase(NFS.NonNullTemplates, "Query_Widgets", "path1", "Query_Widgets_Path1!")]
        [TestCase(NFS.NonNullStrings, "Query_Widgets", "path1", "Query_Widgets_Path1")]
        [TestCase(NFS.NonNullInputStrings, "Query_Widgets", "path1", "Query_Widgets_Path1")]
        [TestCase(NFS.NonNullOutputStrings, "Query_Widgets", "path1", "Query_Widgets_Path1")]
        [TestCase(NFS.NonNullLists, "Query_Widgets", "path1", "Query_Widgets_Path1")]
        [TestCase(NFS.NonNullReferenceTypes, "Query_Widgets", "path1", "Query_Widgets_Path1")]

        // nested templated item returning a virtual type
        [TestCase(NFS.None, "Query_Widgets_Path1", "path2", "Query_Widgets_Path1_Path2")]
        [TestCase(NFS.NonNullTemplates, "Query_Widgets_Path1", "path2", "Query_Widgets_Path1_Path2!")]
        [TestCase(NFS.NonNullStrings, "Query_Widgets_Path1", "path2", "Query_Widgets_Path1_Path2")]
        [TestCase(NFS.NonNullInputStrings, "Query_Widgets_Path1", "path2", "Query_Widgets_Path1_Path2")]
        [TestCase(NFS.NonNullLists, "Query_Widgets_Path1", "path2", "Query_Widgets_Path1_Path2")]
        [TestCase(NFS.NonNullReferenceTypes, "Query_Widgets_Path1", "path2", "Query_Widgets_Path1_Path2")]
        public void VirtualFields_FieldTypeExpressionNullabilityTests(
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

        [TestCase(NFS.None, "intArgument", "Int!")]
        [TestCase(NFS.NonNullTemplates, "intArgument", "Int!")]
        [TestCase(NFS.NonNullStrings, "intArgument", "Int!")]
        [TestCase(NFS.NonNullInputStrings, "intArgument", "Int!")]
        [TestCase(NFS.NonNullOutputStrings, "intArgument", "Int!")]
        [TestCase(NFS.NonNullReferenceTypes, "intArgument", "Int!")]

        [TestCase(NFS.None, "intArgumentFixed", "Int!")]
        [TestCase(NFS.NonNullTemplates, "intArgumentFixed", "Int!")]
        [TestCase(NFS.NonNullStrings, "intArgumentFixed", "Int!")]
        [TestCase(NFS.NonNullInputStrings, "intArgumentFixed", "Int!")]
        [TestCase(NFS.NonNullOutputStrings, "intArgumentFixed", "Int!")]
        [TestCase(NFS.NonNullReferenceTypes, "intArgumentFixed", "Int!")]

        [TestCase(NFS.None, "stringArgument", "String")]
        [TestCase(NFS.NonNullTemplates, "stringArgument", "String")]
        [TestCase(NFS.NonNullStrings, "stringArgument", "String!")]
        [TestCase(NFS.NonNullInputStrings, "stringArgument", "String!")]
        [TestCase(NFS.NonNullOutputStrings, "stringArgument", "String")]
        [TestCase(NFS.NonNullReferenceTypes, "stringArgument", "String")]

        [TestCase(NFS.None, "stringArgumentFixed", "String")]
        [TestCase(NFS.NonNullTemplates, "stringArgumentFixed", "String")]
        [TestCase(NFS.NonNullStrings, "stringArgumentFixed", "String")]
        [TestCase(NFS.NonNullInputStrings, "stringArgumentFixed", "String")]
        [TestCase(NFS.NonNullOutputStrings, "stringArgumentFixed", "String")]
        [TestCase(NFS.NonNullReferenceTypes, "stringArgumentFixed", "String")]

        [TestCase(NFS.None, "inputObjectArgument", "Input_Widget")]
        [TestCase(NFS.NonNullTemplates, "inputObjectArgument", "Input_Widget")]
        [TestCase(NFS.NonNullStrings, "inputObjectArgument", "Input_Widget")]
        [TestCase(NFS.NonNullReferenceTypes, "inputObjectArgument", "Input_Widget!")]

        [TestCase(NFS.None, "inputObjectArgumentFixed", "Input_Widget")]
        [TestCase(NFS.NonNullTemplates, "inputObjectArgumentFixed", "Input_Widget")]
        [TestCase(NFS.NonNullStrings, "inputObjectArgumentFixed", "Input_Widget")]
        [TestCase(NFS.NonNullReferenceTypes, "inputObjectArgumentFixed", "Input_Widget")]
        public void Integration_FieldArgumentTypeExpressionNullabilityTests(
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

        [TestCase(NFS.None, TypeKind.OBJECT, "tripleListProp", "[[[String]]]")]
        [TestCase(NFS.NonNullStrings, TypeKind.OBJECT, "tripleListProp", "[[[String!]]]")]
        [TestCase(NFS.NonNullInputStrings, TypeKind.OBJECT, "tripleListProp", "[[[String]]]")]
        [TestCase(NFS.NonNullOutputStrings, TypeKind.OBJECT, "tripleListProp", "[[[String!]]]")]
        [TestCase(NFS.NonNullReferenceTypes, TypeKind.OBJECT, "tripleListProp", "[[[String]]]")]
        [TestCase(NFS.NonNullLists, TypeKind.OBJECT, "tripleListProp", "[[[String]!]!]!")]
        [TestCase(NFS.NonNullLists | NFS.NonNullStrings, TypeKind.OBJECT, "tripleListProp", "[[[String!]!]!]!")]

        [TestCase(NFS.None, TypeKind.OBJECT, "tripleListPropFixed", "[[[String]]]")]
        [TestCase(NFS.NonNullStrings, TypeKind.OBJECT, "tripleListPropFixed", "[[[String]]]")]
        [TestCase(NFS.NonNullInputStrings, TypeKind.OBJECT, "tripleListPropFixed", "[[[String]]]")]
        [TestCase(NFS.NonNullOutputStrings, TypeKind.OBJECT, "tripleListPropFixed", "[[[String]]]")]
        [TestCase(NFS.NonNullLists, TypeKind.OBJECT, "tripleListPropFixed", "[[[String]]]")]
        [TestCase(NFS.NonNullReferenceTypes, TypeKind.OBJECT, "tripleListPropFixed", "[[[String]]]")]
        [TestCase(NFS.NonNullLists | NFS.NonNullStrings, TypeKind.OBJECT, "tripleListPropFixed", "[[[String]]]")]

        [TestCase(NFS.None, TypeKind.INPUT_OBJECT, "tripleListProp", "[[[String]]]")]
        [TestCase(NFS.NonNullStrings, TypeKind.INPUT_OBJECT, "tripleListProp", "[[[String!]]]")]
        [TestCase(NFS.NonNullInputStrings, TypeKind.INPUT_OBJECT, "tripleListProp", "[[[String!]]]")]
        [TestCase(NFS.NonNullOutputStrings, TypeKind.INPUT_OBJECT, "tripleListProp", "[[[String]]]")]
        [TestCase(NFS.NonNullReferenceTypes, TypeKind.INPUT_OBJECT, "tripleListProp", "[[[String]]]")]
        [TestCase(NFS.NonNullLists, TypeKind.INPUT_OBJECT, "tripleListProp", "[[[String]!]!]!")]
        [TestCase(NFS.NonNullLists | NFS.NonNullStrings, TypeKind.INPUT_OBJECT, "tripleListProp", "[[[String!]!]!]!")]

        [TestCase(NFS.None, TypeKind.INPUT_OBJECT, "tripleListPropFixed", "[[[String]]]")]
        [TestCase(NFS.NonNullStrings, TypeKind.INPUT_OBJECT, "tripleListPropFixed", "[[[String]]]")]
        [TestCase(NFS.NonNullInputStrings, TypeKind.INPUT_OBJECT, "tripleListPropFixed", "[[[String]]]")]
        [TestCase(NFS.NonNullOutputStrings, TypeKind.INPUT_OBJECT, "tripleListPropFixed", "[[[String]]]")]
        [TestCase(NFS.NonNullLists, TypeKind.INPUT_OBJECT, "tripleListPropFixed", "[[[String]]]")]
        [TestCase(NFS.NonNullReferenceTypes, TypeKind.INPUT_OBJECT, "tripleListPropFixed", "[[[String]]]")]
        [TestCase(NFS.NonNullLists | NFS.NonNullStrings, TypeKind.INPUT_OBJECT, "tripleListPropFixed", "[[[String]]]")]
        public void Integration_Objects_FieldTypeExpressionListNullabilityTests(
            NFS strategy,
            TypeKind typeKind,
            string fieldName,
            string expectedTypeExpression)
        {
            var formatter = new GraphSchemaFormatStrategy();
            formatter.NullabilityStrategy = strategy;

            var server = new TestServerBuilder()
                .AddGraphQL(o =>
                {
                    o.AddType<WidgetList>(typeKind);
                    o.DeclarationOptions.SchemaFormatStrategy = formatter;
                })
                .Build();

            if (typeKind == TypeKind.OBJECT)
            {
                var graphType = server.Schema.KnownTypes.FindGraphType(typeof(WidgetList)) as IGraphFieldContainer;
                var field = graphType.Fields[fieldName];
                Assert.AreEqual(expectedTypeExpression, field.TypeExpression.ToString());
            }
            else if (typeKind == TypeKind.INPUT_OBJECT)
            {
                var graphType = server.Schema.KnownTypes.FindGraphType(typeof(WidgetList)) as IInputObjectGraphType;
                var field = graphType.Fields[fieldName];
                Assert.AreEqual(expectedTypeExpression, field.TypeExpression.ToString());
            }
        }

        [TestCase(NFS.None, "intArgument", "[Int!]")]
        [TestCase(NFS.NonNullStrings, "intArgument", "[Int!]")]
        [TestCase(NFS.NonNullLists, "intArgument", "[Int!]!")]
        [TestCase(NFS.NonNullLists | NFS.NonNullStrings, "intArgument", "[Int!]!")]

        [TestCase(NFS.None, "intArgumentFixed", "[Int!]")]
        [TestCase(NFS.NonNullStrings, "intArgumentFixed", "[Int!]")]
        [TestCase(NFS.NonNullLists, "intArgumentFixed", "[Int!]")]
        [TestCase(NFS.NonNullLists | NFS.NonNullStrings, "intArgumentFixed", "[Int!]")]

        [TestCase(NFS.None, "stringArgument", "[String]")]
        [TestCase(NFS.NonNullStrings, "stringArgument", "[String!]")]
        [TestCase(NFS.NonNullInputStrings, "stringArgument", "[String!]")]
        [TestCase(NFS.NonNullOutputStrings, "stringArgument", "[String]")]
        [TestCase(NFS.NonNullLists, "stringArgument", "[String]!")]
        [TestCase(NFS.NonNullLists | NFS.NonNullStrings, "stringArgument", "[String!]!")]

        [TestCase(NFS.None, "stringArgumentFixed", "[String]")]
        [TestCase(NFS.NonNullStrings, "stringArgumentFixed", "[String]")]
        [TestCase(NFS.NonNullInputStrings, "stringArgumentFixed", "[String]")]
        [TestCase(NFS.NonNullOutputStrings, "stringArgumentFixed", "[String]")]
        [TestCase(NFS.NonNullLists, "stringArgumentFixed", "[String]")]
        [TestCase(NFS.NonNullLists | NFS.NonNullStrings, "stringArgumentFixed", "[String]")]

        [TestCase(NFS.None, "inputObjectArgument", "[Input_Widget]")]
        [TestCase(NFS.NonNullStrings, "inputObjectArgument", "[Input_Widget]")]
        [TestCase(NFS.NonNullReferenceTypes, "inputObjectArgument", "[Input_Widget!]")]
        [TestCase(NFS.NonNullLists, "inputObjectArgument", "[Input_Widget]!")]
        [TestCase(NFS.NonNullLists | NFS.NonNullStrings, "inputObjectArgument", "[Input_Widget]!")]
        [TestCase(NFS.NonNullLists | NFS.NonNullReferenceTypes, "inputObjectArgument", "[Input_Widget!]!")]

        [TestCase(NFS.None, "inputObjectArgumentFixed", "[Input_Widget]")]
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

        [Test]
        public void InputObject_NullableField_WithNonNullDefault_DefaultIsRetained_WhenFieldMadeNonNull()
        {
            var formatter = new GraphSchemaFormatStrategy();
            formatter.NullabilityStrategy = NFS.NonNullInputStrings;

            var server = new TestServerBuilder()
                .AddGraphQL(o =>
                {
                    o.AddType<WidgetWithDefaultValue>(TypeKind.INPUT_OBJECT);
                    o.DeclarationOptions.SchemaFormatStrategy = formatter;
                })
                .Build();

            var graphType = server.Schema.KnownTypes.FindGraphType($"Input_{nameof(WidgetWithDefaultValue)}") as IInputObjectGraphType;
            var field = graphType.Fields["stringProp"];

            Assert.AreEqual("String!", field.TypeExpression.ToString());
            Assert.IsTrue(field.HasDefaultValue);
            Assert.AreEqual("default 1", field.DefaultValue.ToString());
        }

        [Test]
        public void FieldArgument_WithNonNullDefault_DefaultIsRetained_WhenFieldMadeNonNull()
        {
            var formatter = new GraphSchemaFormatStrategy();
            formatter.NullabilityStrategy = NFS.NonNullInputStrings;

            var server = new TestServerBuilder()
                .AddGraphQL(o =>
                {
                    o.AddController<WidgetControllerWithDefaultValue>();
                    o.DeclarationOptions.SchemaFormatStrategy = formatter;
                })
                .Build();

            var graphType = server.Schema.KnownTypes.FindGraphType("Query") as IGraphFieldContainer;
            var field = graphType.Fields["retrieveRootWidget"];
            var arg1 = field.Arguments["arg1"];

            Assert.AreEqual("String!", arg1.TypeExpression.ToString());
            Assert.IsTrue(arg1.HasDefaultValue);
            Assert.AreEqual("default 1", arg1.DefaultValue.ToString());
        }
    }
}