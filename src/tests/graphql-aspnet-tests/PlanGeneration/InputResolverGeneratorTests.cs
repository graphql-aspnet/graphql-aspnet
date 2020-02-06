// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.PlanGeneration
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using GraphQL.AspNet.Defaults;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Parsing.Lexing;
    using GraphQL.AspNet.Parsing.Lexing.Source;
    using GraphQL.AspNet.Parsing.NodeMakers.ValueMakers;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs.Values;
    using GraphQL.AspNet.PlanGeneration;
    using GraphQL.AspNet.PlanGeneration.Document.Parts.QueryInputValues;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Tests.CommonHelpers;
    using GraphQL.AspNet.Tests.Framework;
    using NUnit.Framework;

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
            var defaultScalars = new DefaultScalarTypeProvider();

            foreach (var scalarConcreteType in defaultScalars.ConcreteTypes)
            {
                builder.AddGraphType(scalarConcreteType);
            }

            builder.AddGraphType(typeof(TestEnum));
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
            AddInvalidTestCase("ID", "1234");

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
            var generator = new InputResolverMethodGenerator(this.CreateSchema());

            var text = inputText?.AsMemory() ?? ReadOnlyMemory<char>.Empty;
            var source = new SourceText(text);
            var tokenStream = Lexer.Tokenize(source);

            tokenStream.Prime();
            InputValueNode node = null;
            if (!tokenStream.EndOfStream)
            {
                var maker = ValueMakerFactory.CreateMaker(tokenStream.ActiveToken);
                if (maker != null)
                    node = maker.MakeNode(tokenStream) as InputValueNode;
            }

            var inputValue = QueryInputValueFactory.CreateInputValue(node);
            var typeExpression = GraphTypeExpression.FromDeclaration(expressionText);
            var resolver = generator.CreateResolver(typeExpression);
            var result = resolver.Resolve(inputValue);

            Assert.AreEqual(expectedOutput, result);
        }

        [TestCaseSource(nameof(_inputValueResolverTestCases_WithValidData))]
        [SetCulture("de-DE")]
        public void DefaultScalarValueResolvers_WithGermanCulture(string expressionText, string inputText, object expectedOutput)
        {
             DefaultScalarValueResolvers(expressionText, inputText, expectedOutput);
        }

        [TestCaseSource(nameof(_inputValueResolverTestCases_WithInvalidData))]
        public void DefaultScalarValueResolvers_InvalidInputValue(string expressionText, string inputText)
        {
            var generator = new InputResolverMethodGenerator(this.CreateSchema());

            var text = inputText?.AsMemory() ?? ReadOnlyMemory<char>.Empty;
            var source = new SourceText(text);
            var tokenStream = Lexer.Tokenize(source);

            tokenStream.Prime();
            InputValueNode node = null;
            if (!tokenStream.EndOfStream)
            {
                var maker = ValueMakerFactory.CreateMaker(tokenStream.ActiveToken);
                if (maker != null)
                    node = maker.MakeNode(tokenStream) as InputValueNode;
            }

            var inputValue = QueryInputValueFactory.CreateInputValue(node);
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
            var sourceList = new QueryListInputValue(new FakeSyntaxNode());
            sourceList.ListItems.Add(new QueryScalarInputValue(new FakeSyntaxNode(), "15", ScalarValueType.Number));
            sourceList.ListItems.Add(new QueryScalarInputValue(new FakeSyntaxNode(), "12", ScalarValueType.Number));

            var typeExpression = GraphTypeExpression.FromDeclaration("[Int]");
            var generator = new InputResolverMethodGenerator(this.CreateSchema());

            var resolver = generator.CreateResolver(typeExpression);
            var result = resolver.Resolve(sourceList) as IEnumerable;

            CollectionAssert.AreEqual(new List<int> { 15, 12 }, result);
        }

        [Test]
        public void ListOfListValueResolver()
        {
            var innerList1 = new QueryListInputValue(new FakeSyntaxNode());
            innerList1.ListItems.Add(new QueryScalarInputValue(new FakeSyntaxNode(), "15", ScalarValueType.Number));
            innerList1.ListItems.Add(new QueryScalarInputValue(new FakeSyntaxNode(), "12", ScalarValueType.Number));

            var innerList2 = new QueryListInputValue(new FakeSyntaxNode());
            innerList2.ListItems.Add(new QueryScalarInputValue(new FakeSyntaxNode(), "30", ScalarValueType.Number));
            innerList2.ListItems.Add(new QueryScalarInputValue(new FakeSyntaxNode(), "40", ScalarValueType.Number));

            var outerList = new QueryListInputValue(new FakeSyntaxNode());
            outerList.ListItems.Add(innerList1);
            outerList.ListItems.Add(innerList2);

            var typeExpression = GraphTypeExpression.FromDeclaration("[[Int]]");
            var generator = new InputResolverMethodGenerator(this.CreateSchema());

            var resolver = generator.CreateResolver(typeExpression);
            var result = resolver.Resolve(outerList) as IEnumerable;

            CollectionAssert.AreEqual(new List<IEnumerable<int>> { new List<int> { 15, 12 }, new List<int> { 30, 40 } }, result);
        }
    }
}
