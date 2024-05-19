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

    [TestFixture]
    public class SchemaFormatStrategy_NonNullValueTests
    {
        [TestCase(typeof(Widget), TypeKind.INPUT_OBJECT, "stringProp", "String!")]
        [TestCase(typeof(Widget), TypeKind.INPUT_OBJECT, "fixedStringProp", "String")]
        public void DeclareInputFieldValuesAsNonNull_TypeExpressionsAreUpdated(
            Type targetType,
            TypeKind typeKind,
            string fieldName,
            string expectedTypeExpression)
        {
            ExecuteStrategyTest(
              targetType,
              typeKind,
              fieldName,
              string.Empty,
              expectedTypeExpression,
              x => x.DeclareInputFieldValuesAsNonNull(x => true));
        }

        [TestCase(typeof(Widget), TypeKind.OBJECT, "stringProp", "String!")]
        [TestCase(typeof(Widget), TypeKind.OBJECT, "fixedStringProp", "String")]
        [TestCase(typeof(IWidget), TypeKind.INTERFACE, "stringProp", "String!")]
        [TestCase(typeof(IWidget), TypeKind.INTERFACE, "fixedStringProp", "String")]
        [TestCase(typeof(WidgetArgumentController), TypeKind.CONTROLLER, "withArgument", "Widget!")]
        [TestCase(typeof(WidgetArgumentController), TypeKind.CONTROLLER, "withArgumentFixed", "Widget")]
        public void DeclareFieldValuesAsNonNull_TypeExpressionsAreUpdated(
            Type targetType,
            TypeKind typeKind,
            string fieldName,
            string expectedTypeExpression)
        {
            ExecuteStrategyTest(
              targetType,
              typeKind,
              fieldName,
              string.Empty,
              expectedTypeExpression,
              x => x.DeclareFieldValuesAsNonNull(x => true));
        }

        [TestCase(typeof(Widget), TypeKind.OBJECT, "argItem", "arg1", "String!")]
        [TestCase(typeof(Widget), TypeKind.OBJECT, "fixedArgItem", "arg1", "String")]
        [TestCase(typeof(IWidget), TypeKind.INTERFACE, "argItem", "arg1", "String!")]
        [TestCase(typeof(IWidget), TypeKind.INTERFACE, "fixedArgItem", "arg1", "String")]
        [TestCase(typeof(WidgetArgumentController), TypeKind.CONTROLLER, "withArgument", "arg1", "String!")]
        [TestCase(typeof(WidgetArgumentController), TypeKind.CONTROLLER, "withArgumentFixed", "arg1", "String")]
        public void DeclareArgumentValuesAsNonNull_TypeExpressionsAreUpdated(
            Type targetType,
            TypeKind typeKind,
            string fieldName,
            string argName,
            string expectedTypeExpression)
        {
            ExecuteStrategyTest(
                targetType,
                typeKind,
                fieldName,
                argName,
                expectedTypeExpression,
                x => x.DeclareArgumentValuesAsNonNull(x => true));
        }

        private void ExecuteStrategyTest(
                Type targetType,
                TypeKind typeKind,
                string fieldName,
                string argName,
                string expectedTypeExpression,
                Action<SchemaFormatStrategyBuilder> strategyToApply)
        {
            var builder = SchemaFormatStrategyBuilder.Create();
            strategyToApply(builder);

            var strategy = builder.Build();

            var server = new TestServerBuilder()
                .AddGraphQL(o =>
                {
                    o.AddType(targetType, typeKind);
                    o.DeclarationOptions.SchemaFormatStrategy = strategy;
                })
                .Build();

            if (typeKind == TypeKind.CONTROLLER)
            {
                var query = server.Schema.KnownTypes.FindGraphType("Query") as IObjectGraphType;
                var controllerField = query.Fields[fieldName];

                if (!string.IsNullOrWhiteSpace(argName))
                {
                    var controllerFieldArg = controllerField.Arguments[argName];
                    Assert.AreEqual(expectedTypeExpression, controllerFieldArg.TypeExpression.ToString());
                    return;
                }

                Assert.AreEqual(expectedTypeExpression, controllerField.TypeExpression.ToString());
                return;
            }

            var graphType = server.Schema.KnownTypes.FindGraphType(targetType, typeKind);
            if (typeKind == TypeKind.INPUT_OBJECT)
            {
                var field = ((IInputObjectGraphType)graphType).Fields[fieldName];
                Assert.AreEqual(expectedTypeExpression, field.TypeExpression.ToString());
                return;
            }

            var container = graphType as IGraphFieldContainer;
            if (string.IsNullOrWhiteSpace(argName))
            {
                Assert.AreEqual(expectedTypeExpression, container[fieldName].TypeExpression.ToString());
                return;
            }

            var arg = container[fieldName].Arguments[argName];
            Assert.AreEqual(expectedTypeExpression, arg.TypeExpression.ToString());
        }

        [Test]
        public void InputObjectWithNullableField_WithNonNullDefault_DefaultIsRetained_WhenFieldMadeNonNull()
        {
            var strategy = SchemaFormatStrategyBuilder.Create()
                .DeclareInputFieldValuesAsNonNull(x => x.ObjectType == typeof(string))
                .Build();

            var server = new TestServerBuilder()
                .AddGraphQL(o =>
                {
                    o.AddType<WidgetWithDefaultValue>(TypeKind.INPUT_OBJECT);
                    o.DeclarationOptions.SchemaFormatStrategy = strategy;
                })
                .Build();

            var graphType = server.Schema.KnownTypes.FindGraphType($"Input_{nameof(WidgetWithDefaultValue)}") as IInputObjectGraphType;
            var field = graphType.Fields["stringProp"];

            Assert.AreEqual("String!", field.TypeExpression.ToString());
            Assert.IsTrue(field.HasDefaultValue);
            Assert.AreEqual("default 1", field.DefaultValue.ToString());
        }

        [Test]
        public void FieldWithNullableFieldArgument_WithNonNullDefault_DefaultIsRetained_WhenFieldMadeNonNull()
        {
            var strategy = SchemaFormatStrategyBuilder.Create()
                .DeclareArgumentValuesAsNonNull(x => x.ObjectType == typeof(string))
                .Build();

            var server = new TestServerBuilder()
                .AddGraphQL(o =>
                {
                    o.AddController<WidgetControllerWithDefaultValue>();
                    o.DeclarationOptions.SchemaFormatStrategy = strategy;
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