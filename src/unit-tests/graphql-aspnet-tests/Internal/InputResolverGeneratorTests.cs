// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Internal
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using GraphQL.AspNet.Engine;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Execution.Parsing;
    using GraphQL.AspNet.Execution.Parsing.Lexing;
    using GraphQL.AspNet.Execution.Parsing.Lexing.Source;
    using GraphQL.AspNet.Execution.Parsing.NodeBuilders.Inputs;
    using GraphQL.AspNet.Execution.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Execution.QueryPlans.DocumentParts;
    using GraphQL.AspNet.Execution.QueryPlans.DocumentParts.SuppliedValues;
    using GraphQL.AspNet.Execution.Source;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Resolvables;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Internal.Resolvers;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Schemas.TypeSystem.Scalars;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Internal.InputValueNodeTestData;
    using NSubstitute;
    using NUnit.Framework;
    using NUnit.Framework.Constraints;

    [TestFixture]
    public class InputResolverGeneratorTests
    {
        private enum TestEnum
        {
            Value1,
            Value2,
        }

        private ISchema CreateSchema()
        {
            var builder = new TestServerBuilder();
            var defaultScalars = new DefaultScalarGraphTypeProvider();

            foreach (var scalarConcreteType in defaultScalars.ConcreteTypes)
            {
                builder.AddType(scalarConcreteType);
            }

            builder.AddType(typeof(TestEnum));
            builder.AddType(typeof(Telephone), TypeKind.INPUT_OBJECT);
            return builder.Build().Schema;
        }

        private static void AddValidTestCase(string expressionText, string inputText, object expectedOutput)
        {
            _inputValueResolverTestCases_WithValidData = _inputValueResolverTestCases_WithValidData ?? new List<object[]>();
            _inputValueResolverTestCases_WithValidData.Add(new[] { expressionText, inputText, expectedOutput });
        }

        private static void AddInvalidTestCase(string expressionText, string inputText)
        {
            _inputValueResolverTestCases_WithInvalidData = _inputValueResolverTestCases_WithInvalidData ?? new List<object[]>();
            _inputValueResolverTestCases_WithInvalidData.Add(new[] { expressionText, inputText });
        }

        private static List<object[]> _inputValueResolverTestCases_WithValidData;
        private static List<object[]> _inputValueResolverTestCases_WithInvalidData;

        static InputResolverGeneratorTests()
        {
            AddValidTestCase("ULong", "1234", 1234UL);

            AddValidTestCase("Long", "1234", 1234L);
            AddValidTestCase("Long", null, null);
            AddValidTestCase("Long", "9223372036854775807", long.MaxValue);
            AddInvalidTestCase("Long", "9223372036854775808");
            AddValidTestCase("Long", "-9223372036854775808", long.MinValue);
            AddInvalidTestCase("Long", "-9223372036854775809");

            AddValidTestCase("ULong", "1234", 1234UL);
            AddValidTestCase("ULong", null, null);
            AddValidTestCase("ULong", "18446744073709551615", ulong.MaxValue);
            AddInvalidTestCase("ULong", "18446744073709551616");
            AddValidTestCase("ULong", "0", ulong.MinValue);
            AddInvalidTestCase("ULong", "-1");

            AddValidTestCase("Int", "1234", 1234);
            AddValidTestCase("Int", null, null);
            AddValidTestCase("Int", "2147483647", int.MaxValue);
            AddInvalidTestCase("Int", "2147483648");
            AddValidTestCase("Int", "-2147483648", int.MinValue);
            AddInvalidTestCase("Int", "-2147483649");

            AddValidTestCase("UInt", "1234", 1234u);
            AddValidTestCase("UInt", null, null);
            AddValidTestCase("UInt", "4294967295", uint.MaxValue);
            AddInvalidTestCase("UInt", "4294967296");
            AddValidTestCase("UInt", "0", uint.MinValue);
            AddInvalidTestCase("UInt", "-1");

            AddValidTestCase("Float", "12.34", 12.34f);
            AddValidTestCase("Float", "-1234", -1234f);
            AddValidTestCase("Float", "1234.12", 1234.12f);
            AddValidTestCase("Float", "-1234.12", -1234.12f);
            AddValidTestCase("Float", "1234.12e3", 1234.12e3f);
            AddValidTestCase("Float", "-1234.12e3", -1234.12e3f);
            AddValidTestCase("Float", "1234.12E3", 1234.12e3f);
            AddValidTestCase("Float", "-1234.12E3", -1234.12e3f);
            AddValidTestCase("Float", "-1234.12E-3", -1234.12e-3f);
            AddValidTestCase("Float", "-1234.12e-3", -1234.12e-3f);

            AddValidTestCase("Double", "12.34", 12.34d);
            AddValidTestCase("Double", "-1234", -1234d);
            AddValidTestCase("Double", "1234.12", 1234.12d);
            AddValidTestCase("Double", "-1234.12", -1234.12d);
            AddValidTestCase("Double", "1234.12e3", 1234.12e3d);
            AddValidTestCase("Double", "-1234.12e3", -1234.12e3d);
            AddValidTestCase("Double", "1234.12E3", 1234.12e3d);
            AddValidTestCase("Double", "-1234.12E3", -1234.12e3d);
            AddValidTestCase("Double", "-1234.12E-3", -1234.12e-3d);
            AddValidTestCase("Double", "-1234.12e-3", -1234.12e-3d);

            AddValidTestCase("Decimal", "1234", 1234m);
            AddValidTestCase("Decimal", "-1234", -1234m);
            AddValidTestCase("Decimal", "1234.12", 1234.12m);
            AddValidTestCase("Decimal", "-1234.12", -1234.12m);
            AddValidTestCase("Decimal", "-1234.1212321435435354", -1234.1212321435435354m);

            AddValidTestCase("Boolean", "true", true);
            AddValidTestCase("Boolean", "false", false);

            AddValidTestCase("ID", "\"bob\"", new GraphId("bob"));
            AddValidTestCase("ID", "1234", new GraphId("1234"));
            AddInvalidTestCase("ID", "1234.56");

            AddValidTestCase("ID", null, null);
            AddValidTestCase("Uri", "\"http://www.google.com/\"", new Uri("http://www.google.com", UriKind.RelativeOrAbsolute));

            AddValidTestCase("Guid", "\"25bdd5b1-5bbc-4302-9300-2c9294cd6db0\"", new Guid("25bdd5b1-5bbc-4302-9300-2c9294cd6db0"));
            AddInvalidTestCase("Guid", "\"3\"");

            AddValidTestCase("DateTime", "\"2019-09-01\"", new DateTime(2019, 09, 01, 0, 0, 0, DateTimeKind.Utc));
            AddValidTestCase("DateTime", "\"2019-09-01\"", new DateTime(2019, 09, 01, 0, 0, 0, DateTimeKind.Utc));
            AddValidTestCase("DateTime", "\"2006-08-22T06:30:07\"", new DateTime(2006, 8, 22, 6, 30, 7, DateTimeKind.Utc));
            AddValidTestCase("DateTime", "1567438200000", new DateTime(2019, 9, 2, 15, 30, 0, DateTimeKind.Utc));

            AddValidTestCase("String", "\"TestString\"", "TestString");
            AddValidTestCase("String", "\"\"\"TestString\"\"\"", "TestString");
            AddValidTestCase("String", "\"\"", string.Empty);
            AddValidTestCase("String", "\"Test\u00C4String\"", "TestÄString");
            AddInvalidTestCase("String", "12345");
            AddValidTestCase("String", null, null);

            AddValidTestCase("TestEnum", "Value1", TestEnum.Value1);
            AddValidTestCase("TestEnum", "NotRealValue", null);
        }

        [TestCaseSource(nameof(_inputValueResolverTestCases_WithValidData))]
        [SetCulture("en-US")]
        public void DefaultScalarValueResolvers(string expressionText, string inputText, object expectedOutput)
        {
            var owner = Substitute.For<IDocumentPart>();

            var generator = new InputValueResolverMethodGenerator(this.CreateSchema());

            ISuppliedValueDocumentPart inputValue;

            if (inputText != null)
            {
                // simulate having read a supplied input value
                // from a token stream (this is the value normally passed to the resolver)
                var text = inputText.AsSpan();

                var tree = SyntaxTree.WithDocumentRoot();
                var rootNode = tree.RootNode;
                var source = new SourceText(text);
                var tokenStream = Lexer.Tokenize(source);

                tokenStream.Prime();
                SyntaxNode testNode = SyntaxNode.None;
                if (!tokenStream.EndOfStream)
                {
                    var builder = ValueNodeBuilderFactory.CreateBuilder(tokenStream);
                    if (builder != null)
                    {
                        builder.BuildNode(ref tree, ref rootNode, ref tokenStream);
                        testNode = tree.NodePool[rootNode.Coordinates.ChildBlockIndex][rootNode.Coordinates.ChildBlockLength - 1];
                    }
                }

                inputValue = DocumentSuppliedValueFactory.CreateInputValue(
                    source,
                    owner,
                    testNode);
            }
            else
            {
                // imitate the lexer parsing a null value as a valid input
                // rather than trying to lex the value
                var parent = Substitute.For<IDocumentPart>();
                inputValue = new DocumentNullSuppliedValue(
                    parent,
                    new SourceLocation(1, 1, 1));
            }

            var typeExpression = GraphTypeExpression.FromDeclaration(expressionText);
            var resolver = generator.CreateResolver(typeExpression);
            var result = resolver.Resolve(inputValue);

            Assert.AreEqual(expectedOutput, result);
        }

        [TestCaseSource(nameof(_inputValueResolverTestCases_WithValidData))]
        [SetCulture("de-DE")]
        public void DefaultScalarValueResolvers_WithGermanCulture(string expressionText, string inputText, object expectedOutput)
        {
            this.DefaultScalarValueResolvers(expressionText, inputText, expectedOutput);
        }

        [TestCaseSource(nameof(_inputValueResolverTestCases_WithInvalidData))]
        public void DefaultScalarValueResolvers_InvalidInputValue(string expressionText, string inputText)
        {
            var owner = Substitute.For<IDocumentPart>();

            var generator = new InputValueResolverMethodGenerator(this.CreateSchema());

            var text = ReadOnlySpan<char>.Empty;
            if (inputText != null)
                text = inputText.AsSpan();

            var tree = SyntaxTree.WithDocumentRoot();
            var rootNode = tree.RootNode;
            var source = new SourceText(text);
            var tokenStream = Lexer.Tokenize(source);

            tokenStream.Prime();
            SyntaxNode testNode = SyntaxNode.None;

            if (!tokenStream.EndOfStream)
            {
                var builder = ValueNodeBuilderFactory.CreateBuilder(tokenStream);
                if (builder != null)
                {
                    builder.BuildNode(ref tree, ref rootNode, ref tokenStream);
                    testNode = tree.NodePool[rootNode.Coordinates.ChildBlockIndex][rootNode.Coordinates.ChildBlockLength - 1];
                }
            }

            var inputValue = DocumentSuppliedValueFactory.CreateInputValue(
                source,
                owner,
                testNode);

            var typeExpression = GraphTypeExpression.FromDeclaration(expressionText);
            var resolver = generator.CreateResolver(typeExpression);

            Assert.Throws<UnresolvedValueException>(() =>
            {
                resolver.Resolve(inputValue);
            });
        }

        [Test]
        public void BasicListValueResolver()
        {
            var listOwner = Substitute.For<IDocumentPart>();
            listOwner.Parent.Returns(x => null);

            var sourceList = new DocumentListSuppliedValue(listOwner, SourceLocation.None);
            sourceList.Children.Add(new DocumentScalarSuppliedValue(sourceList, "15", ScalarValueType.Unknown, SourceLocation.None));
            sourceList.Children.Add(new DocumentScalarSuppliedValue(sourceList, "12", ScalarValueType.Unknown, SourceLocation.None));

            var typeExpression = GraphTypeExpression.FromDeclaration("[Int]");

            var schema = this.CreateSchema();
            var generator = new InputValueResolverMethodGenerator(schema);

            var resolver = generator.CreateResolver(typeExpression);
            var result = resolver.Resolve(sourceList) as IEnumerable;

            CollectionAssert.AreEqual(new List<int> { 15, 12 }, result);
        }

        [Test]
        public void ListOfListValueResolver()
        {
            var listOwner = Substitute.For<IDocumentPart>();
            listOwner.Parent.Returns(x => null);

            var outerList = new DocumentListSuppliedValue(listOwner, SourceLocation.None);
            var innerList1 = new DocumentListSuppliedValue(outerList, SourceLocation.None);
            innerList1.Children.Add(new DocumentScalarSuppliedValue(innerList1, "15", ScalarValueType.Unknown, SourceLocation.None));
            innerList1.Children.Add(new DocumentScalarSuppliedValue(innerList1, "12", ScalarValueType.Unknown, SourceLocation.None));

            var innerList2 = new DocumentListSuppliedValue(outerList, SourceLocation.None);
            innerList2.Children.Add(new DocumentScalarSuppliedValue(innerList2, "30", ScalarValueType.Unknown, SourceLocation.None));
            innerList2.Children.Add(new DocumentScalarSuppliedValue(innerList2, "40", ScalarValueType.Unknown, SourceLocation.None));

            outerList.Children.Add(innerList1);
            outerList.Children.Add(innerList2);

            var typeExpression = GraphTypeExpression.FromDeclaration("[[Int]]");
            var generator = new InputValueResolverMethodGenerator(this.CreateSchema());

            var resolver = generator.CreateResolver(typeExpression);
            var result = resolver.Resolve(outerList) as IEnumerable;

            CollectionAssert.AreEqual(new List<IEnumerable<int>> { new List<int> { 15, 12 }, new List<int> { 30, 40 } }, result);
        }

        [Test]
        public void InputObjectValueResolver_GeneratesObjectWhenPassed()
        {
            var schema = this.CreateSchema();
            var obj = schema.KnownTypes.FindGraphType("Input_Telephone") as IInputObjectGraphType;

            var owner = Substitute.For<IDocumentPart>();
            owner.Parent.Returns(x => null);

            var complexObject = new DocumentComplexSuppliedValue(owner, SourceLocation.None);
            var idField = new DocumentInputObjectField(complexObject, "id", obj.Fields["id"], SourceLocation.None);
            var brandField = new DocumentInputObjectField(complexObject, "brand", obj.Fields["brand"], SourceLocation.None);

            var idValue = new DocumentScalarSuppliedValue(idField, "15", ScalarValueType.Number, SourceLocation.None);
            var brandValue = new DocumentScalarSuppliedValue(brandField, "\"StarTrucks\"", ScalarValueType.String, SourceLocation.None);

            idField.Children.Add(idValue);
            brandField.Children.Add(brandValue);

            complexObject.Children.Add(idField);
            complexObject.Children.Add(brandField);

            var typeExpression = GraphTypeExpression.FromDeclaration("Input_Telephone");
            var generator = new InputValueResolverMethodGenerator(schema);

            var resolver = generator.CreateResolver(typeExpression);
            var result = resolver.Resolve(complexObject) as Telephone;

            Assert.AreEqual(15, result.Id);
            Assert.AreEqual("StarTrucks", result.Brand);
        }

        [Test]
        public void InputObjectValueResolver_RequiredFieldNotSupplied_ExceptionThrown()
        {
            var schema = this.CreateSchema();
            var obj = schema.KnownTypes.FindGraphType("Input_Telephone") as IInputObjectGraphType;

            var owner = Substitute.For<IDocumentPart>();
            owner.Parent.Returns(x => null);

            var complexObject = new DocumentComplexSuppliedValue(owner, SourceLocation.None);
            var brandField = new DocumentInputObjectField(complexObject, "brand", obj.Fields["brand"], SourceLocation.None);

            var brandValue = new DocumentScalarSuppliedValue(brandField, "\"StarTrucks\"", ScalarValueType.String, SourceLocation.None);

            brandField.Children.Add(brandValue);

            complexObject.Children.Add(brandField);

            var typeExpression = GraphTypeExpression.FromDeclaration("Input_Telephone");
            var generator = new InputValueResolverMethodGenerator(schema);

            var resolver = generator.CreateResolver(typeExpression);
            Assert.Throws<GraphExecutionException>(() =>
            {
                var result = resolver.Resolve(complexObject) as Telephone;
            });
        }

        [Test]
        public void InputObjectValueResolver_ThrowsException_WhenPassedNull()
        {
            var schema = this.CreateSchema();

            var typeExpression = GraphTypeExpression.FromDeclaration("Input_Telephone");
            var generator = new InputValueResolverMethodGenerator(schema);

            var resolver = generator.CreateResolver(typeExpression);

            Assert.Throws<GraphExecutionException>(() =>
            {
                resolver.Resolve(null);
            });
        }

        [Test]
        public void InputObjectValueResolver_ReturnsNull_WhenPassedInterface()
        {
            var schema = this.CreateSchema();

            var testObject = Substitute.For<IResolvableNullValue>();

            var typeExpression = GraphTypeExpression.FromDeclaration("Input_Telephone");
            var generator = new InputValueResolverMethodGenerator(schema);

            var resolver = generator.CreateResolver(typeExpression);
            var result = resolver.Resolve(testObject);

            Assert.IsNull(result);
        }

        [Test]
        public void InputObjectValueResolver_ThrowsException_WhenNotPassedFieldSet()
        {
            var schema = this.CreateSchema();

            var owner = Substitute.For<IDocumentPart>();
            var idValue = new DocumentScalarSuppliedValue(owner, "15", ScalarValueType.String, SourceLocation.None);

            var typeExpression = GraphTypeExpression.FromDeclaration("Input_Telephone");
            var generator = new InputValueResolverMethodGenerator(schema);

            var resolver = generator.CreateResolver(typeExpression);
            Assert.Throws<GraphExecutionException>(() =>
            {
                var result = resolver.Resolve(idValue);
            });
        }

        [Test]
        public void InputObjectValueResolver_WhenNullPasssedToNonNullableAndRequiredField_UnresolvedValueExceptionThrown()
        {
            var schema = this.CreateSchema();
            var obj = schema.KnownTypes.FindGraphType("Input_Telephone") as IInputObjectGraphType;

            var owner = Substitute.For<IDocumentPart>();
            owner.Parent.Returns(x => null);

            var path = new SourcePath();
            path.AddFieldName("topfield");
            owner.Path.Returns(path);

            var complexObject = new DocumentComplexSuppliedValue(owner, SourceLocation.None);
            var idField = new DocumentInputObjectField(complexObject, "id", obj.Fields["id"], SourceLocation.None);
            var brandField = new DocumentInputObjectField(complexObject, "brand", obj.Fields["brand"], SourceLocation.None);

            var idValue = new DocumentNullSuppliedValue(idField, SourceLocation.None);
            var brandValue = new DocumentScalarSuppliedValue(brandField, "\"StarTrucks\"", ScalarValueType.String, SourceLocation.None);

            idField.Children.Add(idValue);
            brandField.Children.Add(brandValue);

            complexObject.Children.Add(idField);
            complexObject.Children.Add(brandField);

            var typeExpression = GraphTypeExpression.FromDeclaration("Input_Telephone");
            var generator = new InputValueResolverMethodGenerator(schema);

            var resolver = generator.CreateResolver(typeExpression);

            Assert.Throws<UnresolvedValueException>(() =>
            {
                var result = resolver.Resolve(complexObject);
            });
        }
    }
}