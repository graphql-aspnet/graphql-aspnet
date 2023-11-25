// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Schemas
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Schemas.TypeSystem.Introspection.Model;
    using GraphQL.AspNet.Tests.Common.CommonHelpers;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Schemas.IntrospectionDefaultValueTestData;
    using NSubstitute;
    using NUnit.Framework;

    [TestFixture]
    public class IntrospectionDefaultValueValidationTests
    {
        private static List<object[]> _inputFieldTestData;
        private static List<object[]> _graphArgumentFieldData;

        static IntrospectionDefaultValueValidationTests()
        {
            _inputFieldTestData = new List<object[]>();
            _graphArgumentFieldData = new List<object[]>();

            _inputFieldTestData.Add(new object[] { "String!", IntrospectionNoDefaultValue.Instance, true });
            _inputFieldTestData.Add(new object[] { "String!", null, false });
            _inputFieldTestData.Add(new object[] { "String", null, true });
            _inputFieldTestData.Add(new object[] { "String", "bob", true });
            _inputFieldTestData.Add(new object[] { "Int!", null, false });
            _inputFieldTestData.Add(new object[] { "Int!", "bob", false });
            _inputFieldTestData.Add(new object[] { "Int!", IntrospectionNoDefaultValue.Instance, true });
            _inputFieldTestData.Add(new object[] { "Int", IntrospectionNoDefaultValue.Instance, true });
            _inputFieldTestData.Add(new object[] { "Int", null, true });
            _inputFieldTestData.Add(new object[] { "Int!", 0, true });
            _inputFieldTestData.Add(new object[] { "Int!", 1, true });
            _inputFieldTestData.Add(new object[] { "Int", 0, true });
            _inputFieldTestData.Add(new object[] { "Int", 1, true });
            _inputFieldTestData.Add(new object[] { "UnknownType", 1, false });
            _inputFieldTestData.Add(new object[] { "UnknownType", IntrospectionNoDefaultValue.Instance, true }); // should not evaluate unknownType
            _inputFieldTestData.Add(new object[] { "TestEnum", (TestEnum)0, true });
            _inputFieldTestData.Add(new object[] { "TestEnum", TestEnum.Value1, true });
            _inputFieldTestData.Add(new object[] { "TestEnum", TestEnum.Value2, true });
            _inputFieldTestData.Add(new object[] { "TestEnum", TestEnum.Value3, false });
            _inputFieldTestData.Add(new object[] { "TestEnumNoDefaultValue", (TestEnumNoDefaultValue)0, false });
            _inputFieldTestData.Add(new object[] { "TestEnumNotIncludedDefault", TestEnumNotIncludedDefault.Value1, false });
            _inputFieldTestData.Add(new object[] { "Input_TwoPropertyObject", new TwoPropertyObject("prop1", 5), true });
            _inputFieldTestData.Add(new object[] { "Input_TwoPropertyObject!", new TwoPropertyObject("prop1", 5), true });
            _inputFieldTestData.Add(new object[] { "Input_TwoPropertyObject", IntrospectionNoDefaultValue.Instance, true });
            _inputFieldTestData.Add(new object[] { "Input_TwoPropertyObject!", IntrospectionNoDefaultValue.Instance, true });
            _inputFieldTestData.Add(new object[] { "Input_TwoPropertyObject!", null, false });
            _inputFieldTestData.Add(new object[] { "[Int!]", 1, false });
            _inputFieldTestData.Add(new object[] { "[Int!]", "bob", false });
            _inputFieldTestData.Add(new object[] { "[Int!]", new int[] { 1, 2, 3 }, true });
            _inputFieldTestData.Add(new object[] { "[Int]", new int?[] { 1, null, 3 }, true });
            _inputFieldTestData.Add(new object[] { "[Int!]", new int?[] { 1, null, 3 }, false });
            _inputFieldTestData.Add(new object[] { "[[Int!]]", new int[] { 1, 2, 3 }, false });
            _inputFieldTestData.Add(new object[] { "[[Int!]]", new int[][] { new int[] { 1 }, new int[] { 2, 3 } }, true });
            _inputFieldTestData.Add(new object[] { "[[Int!]!]", new int[][] { new int[] { 1 }, null }, false });

            _graphArgumentFieldData.Add(new object[] { "String!", null, true, false });
            _graphArgumentFieldData.Add(new object[] { "String!", null, false, true });
            _graphArgumentFieldData.Add(new object[] { "String", null, true, true });
            _graphArgumentFieldData.Add(new object[] { "String", "bob", true, true });
            _graphArgumentFieldData.Add(new object[] { "Int!", null, false, true });
            _graphArgumentFieldData.Add(new object[] { "Int!", "bob", false, true });
            _graphArgumentFieldData.Add(new object[] { "Int!", null, true, false });
            _graphArgumentFieldData.Add(new object[] { "Int", null, true, false });
            _graphArgumentFieldData.Add(new object[] { "Int", null, true, true });
            _graphArgumentFieldData.Add(new object[] { "Int!", 0, true, true });
            _graphArgumentFieldData.Add(new object[] { "Int!", 1, true, true });
            _graphArgumentFieldData.Add(new object[] { "Int", 0, true, true });
            _graphArgumentFieldData.Add(new object[] { "Int", 1, true, true });
            _graphArgumentFieldData.Add(new object[] { "UnknownType", 1, true, false }); // should not evaluate unknown type
            _graphArgumentFieldData.Add(new object[] { "UnknownType", 1, false, true });
            _graphArgumentFieldData.Add(new object[] { "TestEnum", (TestEnum)0, true, true });
            _graphArgumentFieldData.Add(new object[] { "TestEnum", TestEnum.Value1, true, true });
            _graphArgumentFieldData.Add(new object[] { "TestEnum", TestEnum.Value2, true, true });
            _graphArgumentFieldData.Add(new object[] { "TestEnum", TestEnum.Value3, false, true });
            _graphArgumentFieldData.Add(new object[] { "TestEnumNoDefaultValue", (TestEnumNoDefaultValue)0, false, true });
            _graphArgumentFieldData.Add(new object[] { "TestEnumNotIncludedDefault", TestEnumNotIncludedDefault.Value1, false, true });
            _graphArgumentFieldData.Add(new object[] { "Input_TwoPropertyObject", new TwoPropertyObject("prop1", 5), true, true });
            _graphArgumentFieldData.Add(new object[] { "Input_TwoPropertyObject!", new TwoPropertyObject("prop1", 5), true, true });
            _graphArgumentFieldData.Add(new object[] { "Input_TwoPropertyObject", null, true, false });
            _graphArgumentFieldData.Add(new object[] { "Input_TwoPropertyObject!", null, true, false });
            _graphArgumentFieldData.Add(new object[] { "Input_TwoPropertyObject!", null, false, true });
            _graphArgumentFieldData.Add(new object[] { "[Int!]", 1, false, true });
            _graphArgumentFieldData.Add(new object[] { "[Int!]", "bob", false, true });
            _graphArgumentFieldData.Add(new object[] { "[Int!]", new int[] { 1, 2, 3 }, true, true });
            _graphArgumentFieldData.Add(new object[] { "[Int]", new int?[] { 1, null, 3 }, true, true });
            _graphArgumentFieldData.Add(new object[] { "[Int!]", new int?[] { 1, null, 3 }, false, true });
            _graphArgumentFieldData.Add(new object[] { "[[Int!]]", new int[] { 1, 2, 3 }, false, true });
            _graphArgumentFieldData.Add(new object[] { "[[Int!]]", new int[][] { new int[] { 1 }, new int[] { 2, 3 } }, true, true });
            _graphArgumentFieldData.Add(new object[] { "[[Int!]!]", new int[][] { new int[] { 1 }, null }, false, true });
        }

        [TestCaseSource(nameof(_inputFieldTestData))]
        public void InputGraphField_ValidateDefaultValue(
            string typeExpression,
            object defaultValue,
            bool shouldSucceed)
        {
            var server = new TestServerBuilder()
                .AddType<int>()
                .AddType<string>()
                .AddType<TestEnum>()
                .AddType<TestEnumNoDefaultValue>()
                .AddType<TestEnumNotIncludedDefault>()
                .AddType<TwoPropertyObject>(TypeKind.INPUT_OBJECT)
                .Build();

            var graphType = Substitute.For<IGraphType>();

            var introspectedType = new IntrospectedType(graphType);
            var inputField = Substitute.For<IInputGraphField>();
            inputField.ItemPath.Returns(new ItemPath(ItemPathRoots.Types, "inputobject", "field1"));
            inputField.TypeExpression.Returns(GraphTypeExpression.FromDeclaration(typeExpression));

            var introspectedField = new IntrospectedInputValueType(inputField, introspectedType, defaultValue);
            var spectedSchema = new IntrospectedSchema(server.Schema);

            try
            {
                introspectedField.Initialize(spectedSchema);
            }
            catch (GraphTypeDeclarationException)
            {
                if (shouldSucceed)
                    Assert.Fail("Expected default value to initialize correctly");
                return;
            }

            if (!shouldSucceed)
                Assert.Fail("Expected default value to throw exception");
            return;
        }

        [TestCaseSource(nameof(_graphArgumentFieldData))]
        public void GraphArgument_ValidateDefaultValue(
            string typeExpression,
            object defaultValue,
            bool shouldSucceed,
            bool hasDefaultValue)
        {
            var server = new TestServerBuilder()
                .AddType<int>()
                .AddType<string>()
                .AddType<TestEnum>()
                .AddType<TwoPropertyObject>(TypeKind.INPUT_OBJECT)
                .Build();

            var graphType = Substitute.For<IGraphType>();

            var introspectedType = new IntrospectedType(graphType);
            var argument = Substitute.For<IGraphArgument>();
            argument.ItemPath.Returns(new ItemPath(ItemPathRoots.Types, "inputobject", "field1"));
            argument.DefaultValue.Returns(defaultValue);
            argument.TypeExpression.Returns(GraphTypeExpression.FromDeclaration(typeExpression));
            argument.HasDefaultValue.Returns(hasDefaultValue);

            var introspectedField = new IntrospectedInputValueType(argument, introspectedType);
            var spectedSchema = new IntrospectedSchema(server.Schema);

            try
            {
                introspectedField.Initialize(spectedSchema);
            }
            catch (GraphTypeDeclarationException)
            {
                if (shouldSucceed)
                    Assert.Fail("Expected default value to initialize correctly");
                return;
            }

            if (!shouldSucceed)
                Assert.Fail("Expected default value to throw exception");
            return;
        }

        [TestCase("Int")]
        [TestCase("Int!")]
        [TestCase("[Int]")]
        [TestCase("[Int!]")]
        [TestCase("[Int]!")]
        [TestCase("[Int!]!")]
        [TestCase("String")]
        [TestCase("String!")]
        [TestCase("Input_TwoPropertyObject!")]
        [TestCase("Input_TwoPropertyObject")]
        public void InputGraphField_WithNoDefaultValueCOnstructor_ValidateDefaultValue(
            string typeExpression)
        {
            var server = new TestServerBuilder()
                .AddType<int>()
                .AddType<string>()
                .AddType<TestEnum>()
                .AddType<TwoPropertyObject>(TypeKind.INPUT_OBJECT)
                .Build();

            var graphType = Substitute.For<IGraphType>();

            var introspectedType = new IntrospectedType(graphType);
            var inputField = Substitute.For<IInputGraphField>();
            inputField.ItemPath.Returns(new ItemPath(ItemPathRoots.Types, "inputobject", "field1"));
            inputField.TypeExpression.Returns(GraphTypeExpression.FromDeclaration(typeExpression));

            var introspectedField = new IntrospectedInputValueType(inputField, introspectedType);
            var spectedSchema = new IntrospectedSchema(server.Schema);

            // should never throw an exception
            introspectedField.Initialize(spectedSchema);
        }
    }
}
