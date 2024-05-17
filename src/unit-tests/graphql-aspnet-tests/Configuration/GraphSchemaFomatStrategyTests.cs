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
    using NullRules = GraphQL.AspNet.Configuration.Formatting.TypeExpressionNullabilityFormatRules;

    [TestFixture]
    public class GraphSchemaFomatStrategyTests
    {
        // value type, not fixed, object graph type
        [TestCase(NullRules.None, typeof(Widget), "intProp", "Int!")]
        [TestCase(NullRules.NonNullStrings, typeof(Widget), "intProp", "Int!")]
        [TestCase(NullRules.NonNullLists, typeof(Widget), "intProp", "Int!")]
        [TestCase(NullRules.NonNullIntermediateTypes, typeof(Widget), "intProp", "Int!")]
        [TestCase(NullRules.NonNullReferenceTypes, typeof(Widget), "intProp", "Int!")]

        // value type, fixed as nullable, object graph type
        [TestCase(NullRules.None, typeof(Widget), "fixedIntPropAsNullable", "Int")]
        [TestCase(NullRules.NonNullStrings, typeof(Widget), "fixedIntPropAsNullable", "Int")]
        [TestCase(NullRules.NonNullLists, typeof(Widget), "fixedIntPropAsNullable", "Int")]
        [TestCase(NullRules.NonNullIntermediateTypes, typeof(Widget), "fixedIntPropAsNullable", "Int")]
        [TestCase(NullRules.NonNullReferenceTypes, typeof(Widget), "fixedIntPropAsNullable", "Int")]

        // value type, fixed as not nullable, object graph type
        [TestCase(NullRules.None, typeof(Widget), "fixedIntPropAsNotNullable", "Int!")]
        [TestCase(NullRules.NonNullStrings, typeof(Widget), "fixedIntPropAsNotNullable", "Int!")]
        [TestCase(NullRules.NonNullLists, typeof(Widget), "fixedIntPropAsNotNullable", "Int!")]
        [TestCase(NullRules.NonNullIntermediateTypes, typeof(Widget), "fixedIntPropAsNotNullable", "Int!")]
        [TestCase(NullRules.NonNullReferenceTypes, typeof(Widget), "fixedIntPropAsNotNullable", "Int!")]

        // string type, not fixed, object graph type
        [TestCase(NullRules.None, typeof(Widget), "stringProp", "String")]
        [TestCase(NullRules.NonNullStrings, typeof(Widget), "stringProp", "String!")]
        [TestCase(NullRules.NonNullInputStrings, typeof(Widget), "stringProp", "String")]
        [TestCase(NullRules.NonNullOutputStrings, typeof(Widget), "stringProp", "String!")]
        [TestCase(NullRules.NonNullLists, typeof(Widget), "stringProp", "String")]
        [TestCase(NullRules.NonNullIntermediateTypes, typeof(Widget), "stringProp", "String")]
        [TestCase(NullRules.NonNullReferenceTypes, typeof(Widget), "stringProp", "String")]

        // string, fixed, object graph type
        [TestCase(NullRules.None, typeof(Widget), "fixedStringProp", "String")]
        [TestCase(NullRules.NonNullStrings, typeof(Widget), "fixedStringProp", "String")]
        [TestCase(NullRules.NonNullInputStrings, typeof(Widget), "fixedStringProp", "String")]
        [TestCase(NullRules.NonNullOutputStrings, typeof(Widget), "fixedStringProp", "String")]
        [TestCase(NullRules.NonNullLists, typeof(Widget), "fixedStringProp", "String")]
        [TestCase(NullRules.NonNullIntermediateTypes, typeof(Widget), "fixedStringProp", "String")]
        [TestCase(NullRules.NonNullReferenceTypes, typeof(Widget), "fixedStringProp", "String")]

        // object reference, not fixed, object graph type
        [TestCase(NullRules.None, typeof(Widget), "referenceProp", "Widget")]
        [TestCase(NullRules.NonNullStrings, typeof(Widget), "referenceProp", "Widget")]
        [TestCase(NullRules.NonNullLists, typeof(Widget), "referenceProp", "Widget")]
        [TestCase(NullRules.NonNullIntermediateTypes, typeof(Widget), "referenceProp", "Widget")]
        [TestCase(NullRules.NonNullReferenceTypes, typeof(Widget), "referenceProp", "Widget!")]

        // object reference, fixed, object graph type
        [TestCase(NullRules.None, typeof(Widget), "fixedReferenceProp", "Widget")]
        [TestCase(NullRules.NonNullStrings, typeof(Widget), "fixedReferenceProp", "Widget")]
        [TestCase(NullRules.NonNullLists, typeof(Widget), "fixedReferenceProp", "Widget")]
        [TestCase(NullRules.NonNullIntermediateTypes, typeof(Widget), "fixedReferenceProp", "Widget")]
        [TestCase(NullRules.NonNullReferenceTypes, typeof(Widget), "fixedReferenceProp", "Widget")]

        // value type, not fixed, interface graph type
        [TestCase(NullRules.None, typeof(IWidgetInterface), "intProp", "Int!")]
        [TestCase(NullRules.NonNullStrings, typeof(IWidgetInterface), "intProp", "Int!")]
        [TestCase(NullRules.NonNullLists, typeof(IWidgetInterface), "intProp", "Int!")]
        [TestCase(NullRules.NonNullIntermediateTypes, typeof(IWidgetInterface), "intProp", "Int!")]
        [TestCase(NullRules.NonNullReferenceTypes, typeof(IWidgetInterface), "intProp", "Int!")]

        // value type, fixed as nullable, interface graph type
        [TestCase(NullRules.None, typeof(IWidgetInterface), "fixedIntPropAsNullable", "Int")]
        [TestCase(NullRules.NonNullStrings, typeof(IWidgetInterface), "fixedIntPropAsNullable", "Int")]
        [TestCase(NullRules.NonNullLists, typeof(IWidgetInterface), "fixedIntPropAsNullable", "Int")]
        [TestCase(NullRules.NonNullIntermediateTypes, typeof(IWidgetInterface), "fixedIntPropAsNullable", "Int")]
        [TestCase(NullRules.NonNullReferenceTypes, typeof(IWidgetInterface), "fixedIntPropAsNullable", "Int")]

        // value type, fixed as not nullable, interface graph type
        [TestCase(NullRules.None, typeof(IWidgetInterface), "fixedIntPropAsNotNullable", "Int!")]
        [TestCase(NullRules.NonNullStrings, typeof(IWidgetInterface), "fixedIntPropAsNotNullable", "Int!")]
        [TestCase(NullRules.NonNullLists, typeof(IWidgetInterface), "fixedIntPropAsNotNullable", "Int!")]
        [TestCase(NullRules.NonNullIntermediateTypes, typeof(IWidgetInterface), "fixedIntPropAsNotNullable", "Int!")]
        [TestCase(NullRules.NonNullReferenceTypes, typeof(IWidgetInterface), "fixedIntPropAsNotNullable", "Int!")]

        // string type, not fixed, interface graph type
        [TestCase(NullRules.None, typeof(IWidgetInterface), "stringProp", "String")]
        [TestCase(NullRules.NonNullStrings, typeof(IWidgetInterface), "stringProp", "String!")]
        [TestCase(NullRules.NonNullInputStrings, typeof(IWidgetInterface), "stringProp", "String")]
        [TestCase(NullRules.NonNullOutputStrings, typeof(IWidgetInterface), "stringProp", "String!")]
        [TestCase(NullRules.NonNullLists, typeof(IWidgetInterface), "stringProp", "String")]
        [TestCase(NullRules.NonNullIntermediateTypes, typeof(IWidgetInterface), "stringProp", "String")]
        [TestCase(NullRules.NonNullReferenceTypes, typeof(IWidgetInterface), "stringProp", "String")]

        // string, fixed, interface graph type
        [TestCase(NullRules.None, typeof(IWidgetInterface), "fixedStringProp", "String")]
        [TestCase(NullRules.NonNullStrings, typeof(IWidgetInterface), "fixedStringProp", "String")]
        [TestCase(NullRules.NonNullInputStrings, typeof(IWidgetInterface), "fixedStringProp", "String")]
        [TestCase(NullRules.NonNullOutputStrings, typeof(IWidgetInterface), "fixedStringProp", "String")]
        [TestCase(NullRules.NonNullLists, typeof(IWidgetInterface), "fixedStringProp", "String")]
        [TestCase(NullRules.NonNullIntermediateTypes, typeof(IWidgetInterface), "fixedStringProp", "String")]
        [TestCase(NullRules.NonNullReferenceTypes, typeof(IWidgetInterface), "fixedStringProp", "String")]

        // object reference, not fixed, interface graph type
        [TestCase(NullRules.None, typeof(IWidgetInterface), "referenceProp", "IWidgetInterface")]
        [TestCase(NullRules.NonNullStrings, typeof(IWidgetInterface), "referenceProp", "IWidgetInterface")]
        [TestCase(NullRules.NonNullLists, typeof(IWidgetInterface), "referenceProp", "IWidgetInterface")]
        [TestCase(NullRules.NonNullIntermediateTypes, typeof(IWidgetInterface), "referenceProp", "IWidgetInterface")]
        [TestCase(NullRules.NonNullReferenceTypes, typeof(IWidgetInterface), "referenceProp", "IWidgetInterface!")]

        // object reference, fixed, interface graph type
        [TestCase(NullRules.None, typeof(IWidgetInterface), "fixedReferenceProp", "IWidgetInterface")]
        [TestCase(NullRules.NonNullStrings, typeof(IWidgetInterface), "fixedReferenceProp", "IWidgetInterface")]
        [TestCase(NullRules.NonNullLists, typeof(IWidgetInterface), "fixedReferenceProp", "IWidgetInterface")]
        [TestCase(NullRules.NonNullIntermediateTypes, typeof(IWidgetInterface), "fixedReferenceProp", "IWidgetInterface")]
        [TestCase(NullRules.NonNullReferenceTypes, typeof(IWidgetInterface), "fixedReferenceProp", "IWidgetInterface")]
        public void Integration_ConcreteObjectTypes_FieldTypeExpressionTests(
            NullRules strategy,
            Type concreteType,
            string fieldName,
            string expectedTypeExpression)
        {
            var formatter = new SchemaFormatStrategy();
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
        [TestCase(NullRules.None, "Query_Widgets", "path1", "Query_Widgets_Path1")]
        [TestCase(NullRules.NonNullIntermediateTypes, "Query_Widgets", "path1", "Query_Widgets_Path1!")]
        [TestCase(NullRules.NonNullStrings, "Query_Widgets", "path1", "Query_Widgets_Path1")]
        [TestCase(NullRules.NonNullInputStrings, "Query_Widgets", "path1", "Query_Widgets_Path1")]
        [TestCase(NullRules.NonNullOutputStrings, "Query_Widgets", "path1", "Query_Widgets_Path1")]
        [TestCase(NullRules.NonNullLists, "Query_Widgets", "path1", "Query_Widgets_Path1")]
        [TestCase(NullRules.NonNullReferenceTypes, "Query_Widgets", "path1", "Query_Widgets_Path1")]

        // nested templated item returning a virtual type
        [TestCase(NullRules.None, "Query_Widgets_Path1", "path2", "Query_Widgets_Path1_Path2")]
        [TestCase(NullRules.NonNullIntermediateTypes, "Query_Widgets_Path1", "path2", "Query_Widgets_Path1_Path2!")]
        [TestCase(NullRules.NonNullStrings, "Query_Widgets_Path1", "path2", "Query_Widgets_Path1_Path2")]
        [TestCase(NullRules.NonNullInputStrings, "Query_Widgets_Path1", "path2", "Query_Widgets_Path1_Path2")]
        [TestCase(NullRules.NonNullLists, "Query_Widgets_Path1", "path2", "Query_Widgets_Path1_Path2")]
        [TestCase(NullRules.NonNullReferenceTypes, "Query_Widgets_Path1", "path2", "Query_Widgets_Path1_Path2")]
        public void VirtualFields_FieldTypeExpressionNullabilityTests(
            NullRules strategy,
            string graphTypeName,
            string fieldName,
            string expectedTypeExpression)
        {
            var formatter = new SchemaFormatStrategy();
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

        [TestCase(NullRules.None, "intArgument", "Int!")]
        [TestCase(NullRules.NonNullIntermediateTypes, "intArgument", "Int!")]
        [TestCase(NullRules.NonNullStrings, "intArgument", "Int!")]
        [TestCase(NullRules.NonNullInputStrings, "intArgument", "Int!")]
        [TestCase(NullRules.NonNullOutputStrings, "intArgument", "Int!")]
        [TestCase(NullRules.NonNullReferenceTypes, "intArgument", "Int!")]

        [TestCase(NullRules.None, "intArgumentFixed", "Int!")]
        [TestCase(NullRules.NonNullIntermediateTypes, "intArgumentFixed", "Int!")]
        [TestCase(NullRules.NonNullStrings, "intArgumentFixed", "Int!")]
        [TestCase(NullRules.NonNullInputStrings, "intArgumentFixed", "Int!")]
        [TestCase(NullRules.NonNullOutputStrings, "intArgumentFixed", "Int!")]
        [TestCase(NullRules.NonNullReferenceTypes, "intArgumentFixed", "Int!")]

        [TestCase(NullRules.None, "stringArgument", "String")]
        [TestCase(NullRules.NonNullIntermediateTypes, "stringArgument", "String")]
        [TestCase(NullRules.NonNullStrings, "stringArgument", "String!")]
        [TestCase(NullRules.NonNullInputStrings, "stringArgument", "String!")]
        [TestCase(NullRules.NonNullOutputStrings, "stringArgument", "String")]
        [TestCase(NullRules.NonNullReferenceTypes, "stringArgument", "String")]

        [TestCase(NullRules.None, "stringArgumentFixed", "String")]
        [TestCase(NullRules.NonNullIntermediateTypes, "stringArgumentFixed", "String")]
        [TestCase(NullRules.NonNullStrings, "stringArgumentFixed", "String")]
        [TestCase(NullRules.NonNullInputStrings, "stringArgumentFixed", "String")]
        [TestCase(NullRules.NonNullOutputStrings, "stringArgumentFixed", "String")]
        [TestCase(NullRules.NonNullReferenceTypes, "stringArgumentFixed", "String")]

        [TestCase(NullRules.None, "inputObjectArgument", "Input_Widget")]
        [TestCase(NullRules.NonNullIntermediateTypes, "inputObjectArgument", "Input_Widget")]
        [TestCase(NullRules.NonNullStrings, "inputObjectArgument", "Input_Widget")]
        [TestCase(NullRules.NonNullInputStrings, "inputObjectArgument", "Input_Widget")]
        [TestCase(NullRules.NonNullOutputStrings, "inputObjectArgument", "Input_Widget")]
        [TestCase(NullRules.NonNullReferenceTypes, "inputObjectArgument", "Input_Widget!")]

        [TestCase(NullRules.None, "inputObjectArgumentFixed", "Input_Widget")]
        [TestCase(NullRules.NonNullIntermediateTypes, "inputObjectArgumentFixed", "Input_Widget")]
        [TestCase(NullRules.NonNullStrings, "inputObjectArgumentFixed", "Input_Widget")]
        [TestCase(NullRules.NonNullInputStrings, "inputObjectArgumentFixed", "Input_Widget")]
        [TestCase(NullRules.NonNullOutputStrings, "inputObjectArgumentFixed", "Input_Widget")]
        [TestCase(NullRules.NonNullReferenceTypes, "inputObjectArgumentFixed", "Input_Widget")]
        public void Integration_FieldArgumentTypeExpressionNullabilityTests(
                NullRules strategy,
                string fieldName,
                string expectedTypeExpression)
        {
            var formatter = new SchemaFormatStrategy();
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

        [TestCase(NullRules.None, TypeKind.OBJECT, "tripleListProp", "[[[String]]]")]
        [TestCase(NullRules.NonNullStrings, TypeKind.OBJECT, "tripleListProp", "[[[String!]]]")]
        [TestCase(NullRules.NonNullInputStrings, TypeKind.OBJECT, "tripleListProp", "[[[String]]]")]
        [TestCase(NullRules.NonNullOutputStrings, TypeKind.OBJECT, "tripleListProp", "[[[String!]]]")]
        [TestCase(NullRules.NonNullReferenceTypes, TypeKind.OBJECT, "tripleListProp", "[[[String]]]")]
        [TestCase(NullRules.NonNullLists, TypeKind.OBJECT, "tripleListProp", "[[[String]!]!]!")]
        [TestCase(NullRules.NonNullLists | NullRules.NonNullStrings, TypeKind.OBJECT, "tripleListProp", "[[[String!]!]!]!")]

        [TestCase(NullRules.None, TypeKind.OBJECT, "tripleListPropFixed", "[[[String]]]")]
        [TestCase(NullRules.NonNullStrings, TypeKind.OBJECT, "tripleListPropFixed", "[[[String]]]")]
        [TestCase(NullRules.NonNullInputStrings, TypeKind.OBJECT, "tripleListPropFixed", "[[[String]]]")]
        [TestCase(NullRules.NonNullOutputStrings, TypeKind.OBJECT, "tripleListPropFixed", "[[[String]]]")]
        [TestCase(NullRules.NonNullLists, TypeKind.OBJECT, "tripleListPropFixed", "[[[String]]]")]
        [TestCase(NullRules.NonNullReferenceTypes, TypeKind.OBJECT, "tripleListPropFixed", "[[[String]]]")]
        [TestCase(NullRules.NonNullLists | NullRules.NonNullStrings, TypeKind.OBJECT, "tripleListPropFixed", "[[[String]]]")]

        [TestCase(NullRules.None, TypeKind.INPUT_OBJECT, "tripleListProp", "[[[String]]]")]
        [TestCase(NullRules.NonNullStrings, TypeKind.INPUT_OBJECT, "tripleListProp", "[[[String!]]]")]
        [TestCase(NullRules.NonNullInputStrings, TypeKind.INPUT_OBJECT, "tripleListProp", "[[[String!]]]")]
        [TestCase(NullRules.NonNullOutputStrings, TypeKind.INPUT_OBJECT, "tripleListProp", "[[[String]]]")]
        [TestCase(NullRules.NonNullReferenceTypes, TypeKind.INPUT_OBJECT, "tripleListProp", "[[[String]]]")]
        [TestCase(NullRules.NonNullLists, TypeKind.INPUT_OBJECT, "tripleListProp", "[[[String]!]!]!")]
        [TestCase(NullRules.NonNullLists | NullRules.NonNullStrings, TypeKind.INPUT_OBJECT, "tripleListProp", "[[[String!]!]!]!")]

        [TestCase(NullRules.None, TypeKind.INPUT_OBJECT, "tripleListPropFixed", "[[[String]]]")]
        [TestCase(NullRules.NonNullStrings, TypeKind.INPUT_OBJECT, "tripleListPropFixed", "[[[String]]]")]
        [TestCase(NullRules.NonNullInputStrings, TypeKind.INPUT_OBJECT, "tripleListPropFixed", "[[[String]]]")]
        [TestCase(NullRules.NonNullOutputStrings, TypeKind.INPUT_OBJECT, "tripleListPropFixed", "[[[String]]]")]
        [TestCase(NullRules.NonNullLists, TypeKind.INPUT_OBJECT, "tripleListPropFixed", "[[[String]]]")]
        [TestCase(NullRules.NonNullReferenceTypes, TypeKind.INPUT_OBJECT, "tripleListPropFixed", "[[[String]]]")]
        [TestCase(NullRules.NonNullLists | NullRules.NonNullStrings, TypeKind.INPUT_OBJECT, "tripleListPropFixed", "[[[String]]]")]
        public void Integration_Objects_FieldTypeExpressionListNullabilityTests(
            NullRules strategy,
            TypeKind typeKind,
            string fieldName,
            string expectedTypeExpression)
        {
            var formatter = new SchemaFormatStrategy();
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

        [TestCase(NullRules.None, "intArgument", "[Int!]")]
        [TestCase(NullRules.NonNullStrings, "intArgument", "[Int!]")]
        [TestCase(NullRules.NonNullLists, "intArgument", "[Int!]!")]
        [TestCase(NullRules.NonNullLists | NullRules.NonNullStrings, "intArgument", "[Int!]!")]

        [TestCase(NullRules.None, "intArgumentFixed", "[Int!]")]
        [TestCase(NullRules.NonNullStrings, "intArgumentFixed", "[Int!]")]
        [TestCase(NullRules.NonNullLists, "intArgumentFixed", "[Int!]")]
        [TestCase(NullRules.NonNullLists | NullRules.NonNullStrings, "intArgumentFixed", "[Int!]")]

        [TestCase(NullRules.None, "stringArgument", "[String]")]
        [TestCase(NullRules.NonNullStrings, "stringArgument", "[String!]")]
        [TestCase(NullRules.NonNullInputStrings, "stringArgument", "[String!]")]
        [TestCase(NullRules.NonNullOutputStrings, "stringArgument", "[String]")]
        [TestCase(NullRules.NonNullLists, "stringArgument", "[String]!")]
        [TestCase(NullRules.NonNullLists | NullRules.NonNullStrings, "stringArgument", "[String!]!")]

        [TestCase(NullRules.None, "stringArgumentFixed", "[String]")]
        [TestCase(NullRules.NonNullStrings, "stringArgumentFixed", "[String]")]
        [TestCase(NullRules.NonNullInputStrings, "stringArgumentFixed", "[String]")]
        [TestCase(NullRules.NonNullOutputStrings, "stringArgumentFixed", "[String]")]
        [TestCase(NullRules.NonNullLists, "stringArgumentFixed", "[String]")]
        [TestCase(NullRules.NonNullLists | NullRules.NonNullStrings, "stringArgumentFixed", "[String]")]

        [TestCase(NullRules.None, "inputObjectArgument", "[Input_Widget]")]
        [TestCase(NullRules.NonNullStrings, "inputObjectArgument", "[Input_Widget]")]
        [TestCase(NullRules.NonNullReferenceTypes, "inputObjectArgument", "[Input_Widget!]")]
        [TestCase(NullRules.NonNullLists, "inputObjectArgument", "[Input_Widget]!")]
        [TestCase(NullRules.NonNullLists | NullRules.NonNullStrings, "inputObjectArgument", "[Input_Widget]!")]
        [TestCase(NullRules.NonNullLists | NullRules.NonNullReferenceTypes, "inputObjectArgument", "[Input_Widget!]!")]

        [TestCase(NullRules.None, "inputObjectArgumentFixed", "[Input_Widget]")]
        [TestCase(NullRules.NonNullStrings, "inputObjectArgumentFixed", "[Input_Widget]")]
        [TestCase(NullRules.NonNullReferenceTypes, "inputObjectArgumentFixed", "[Input_Widget]")]
        [TestCase(NullRules.NonNullLists, "inputObjectArgumentFixed", "[Input_Widget]")]
        [TestCase(NullRules.NonNullLists | NullRules.NonNullStrings, "inputObjectArgumentFixed", "[Input_Widget]")]
        [TestCase(NullRules.NonNullLists | NullRules.NonNullReferenceTypes, "inputObjectArgumentFixed", "[Input_Widget]")]
        public void Integration_Controllers_FieldTypeExpressionListNullabilityTests(
            NullRules strategy,
            string fieldName,
            string expectedTypeExpression)
        {
            var formatter = new SchemaFormatStrategy();
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
            var formatter = new SchemaFormatStrategy();
            formatter.NullabilityStrategy = NullRules.NonNullInputStrings;

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
            var formatter = new SchemaFormatStrategy();
            formatter.NullabilityStrategy = NullRules.NonNullInputStrings;

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