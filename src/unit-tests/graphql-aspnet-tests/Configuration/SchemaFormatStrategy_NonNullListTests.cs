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
    public class SchemaFormatStrategy_NonNullListTests
    {
        [TestCase(typeof(WidgetList), TypeKind.INPUT_OBJECT, "tripleListProp", "[[[String]!]!]!")]
        [TestCase(typeof(WidgetList), TypeKind.INPUT_OBJECT, "tripleListPropFixed", "[[[String]]]")]
        public void DeclareInputFieldListsAsNonNull_TypeExpressionsAreUpdated(
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
              x => x.DeclareInputFieldListsAsNonNull(x => true));
        }

        [TestCase(typeof(WidgetList), TypeKind.OBJECT, "tripleListProp", "[[[String]!]!]!")]
        [TestCase(typeof(WidgetList), TypeKind.OBJECT, "tripleListPropFixed", "[[[String]]]")]
        [TestCase(typeof(IWidgetList), TypeKind.INTERFACE, "tripleListProp", "[[[String]!]!]!")]
        [TestCase(typeof(IWidgetList), TypeKind.INTERFACE, "tripleListPropFixed", "[[[String]]]")]
        [TestCase(typeof(WidgetListController), TypeKind.CONTROLLER, "returnedStringList", "[String]!")]
        [TestCase(typeof(WidgetListController), TypeKind.CONTROLLER, "returnedStringListFixed", "[String]")]
        public void DeclareFieldListsAsNonNull_TypeExpressionsAreUpdated(
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
              x => x.DeclareFieldListsAsNonNull(x => true));
        }

        [TestCase(typeof(WidgetList), TypeKind.OBJECT, "tripleListArg", "arg1", "[[[String]!]!]!")]
        [TestCase(typeof(WidgetList), TypeKind.OBJECT, "tripleListArgFixed", "arg1", "[[[String]]]")]
        [TestCase(typeof(IWidgetList), TypeKind.INTERFACE, "tripleListArg", "arg1", "[[[String]!]!]!")]
        [TestCase(typeof(IWidgetList), TypeKind.INTERFACE, "tripleListArgFixed", "arg1", "[[[String]]]")]
        [TestCase(typeof(WidgetListController), TypeKind.CONTROLLER, "stringListArgument", "arg1", "[String]!")]
        [TestCase(typeof(WidgetListController), TypeKind.CONTROLLER, "stringListArgumentFixed", "arg1", "[String]")]
        public void DeclareArgumentListsAsNonNull_TypeExpressionsAreUpdated(
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
                x => x.DeclareArgumentListsAsNonNull(x => true));
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
    }
}