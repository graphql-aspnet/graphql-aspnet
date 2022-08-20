// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Defaults.TypeMakers
{
    using System;
    using System.Linq;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Configuration.Formatting;
    using GraphQL.AspNet.Defaults.TypeMakers;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Defaults.TypeMakers.TestData;
    using GraphQL.AspNet.Tests.Framework;
    using NUnit.Framework;

    [TestFixture]
    public class EnumGraphTypeMakerTests : GraphTypeMakerTestBase
    {
        [Test]
        public void Parse_EnumWithUndeclaredValues_WhenConfigRequiresDeclaration_DoesntIncludeUndeclared_InGraphType()
        {
            var schema = new TestServerBuilder().AddGraphQL(o =>
                        {
                            o.DeclarationOptions.FieldDeclarationRequirements = TemplateDeclarationRequirements.EnumValue;
                        })
                        .Build()
                        .Schema;

            var maker = new EnumGraphTypeMaker(schema);

            var graphType = maker.CreateGraphType(typeof(EnumWithUndeclaredValues)).GraphType as IEnumGraphType;
            Assert.AreEqual(2, graphType.Values.Count);
            Assert.IsTrue(graphType.Values.ContainsKey("DECLAREDVALUE1"));
            Assert.IsTrue(graphType.Values.ContainsKey("VALUE_AWESOME"));
        }

        [Test]
        public void Parse_EnumWithUndeclaredValues_WhenConfigDoesNotRequireDeclaration_DoesIncludeUndeclared_InGraphType()
        {
            var schema = new TestServerBuilder().AddGraphQL(o =>
            {
                o.DeclarationOptions.FieldDeclarationRequirements = TemplateDeclarationRequirements.None;
            })
                .Build()
                .Schema;

            var maker = new EnumGraphTypeMaker(schema);
            var graphType = maker.CreateGraphType(typeof(EnumWithUndeclaredValues)).GraphType as IEnumGraphType;

            Assert.AreEqual(3, graphType.Values.Count);
            Assert.IsTrue(graphType.Values.ContainsKey("DECLAREDVALUE1"));

            Assert.IsTrue(graphType.Values.ContainsKey("VALUE_AWESOME"));
            Assert.IsFalse(graphType.Values.ContainsKey("DECLAREDVALUE2"));

            Assert.IsTrue(graphType.Values.ContainsKey("UNDECLAREDVALUE1"));
        }

        [Test]
        public void Parse_EnumWithCustomGraphTypeName_YieldsName_InGraphType()
        {
            var schema = new GraphSchema();

            var maker = new EnumGraphTypeMaker(schema);
            var graphType = maker.CreateGraphType(typeof(EnumWithGraphName)).GraphType as IEnumGraphType;
            Assert.AreEqual("ValidGraphName", graphType.Name);
        }

        [Test]
        public void CreateGraphType_ParsesAsExpected()
        {
            var schema = new TestServerBuilder()
                .AddGraphQL(o =>
                {
                    o.DeclarationOptions.GraphNamingFormatter =
                        new GraphNameFormatter(enumValueStrategy: GraphNameFormatStrategy.NoChanges);
                })
                .Build()
                .Schema;

            var template = TemplateHelper.CreateEnumTemplate<EnumWithDescriptionOnValues>();

            var maker = new EnumGraphTypeMaker(schema);
            var graphType = maker.CreateGraphType(typeof(EnumWithDescriptionOnValues)).GraphType as IEnumGraphType;

            Assert.IsNotNull(graphType);
            Assert.AreEqual(template.Name, graphType.Name);

            Assert.IsTrue(graphType is EnumGraphType);
            Assert.AreEqual(4, ((EnumGraphType)graphType).Values.Count);
        }

        [Test]
        public void AppliedDirectives_TransferFromTemplate()
        {
            var schema = new GraphSchema();

            var maker = new EnumGraphTypeMaker(schema);
            var graphType = maker.CreateGraphType(typeof(EnumWithDirective)).GraphType as IEnumGraphType;

            Assert.AreEqual(graphType, graphType.AppliedDirectives.Parent);

            var appliedDirective = graphType.AppliedDirectives.Single();
            Assert.IsNotNull(appliedDirective);
            Assert.AreEqual(typeof(DirectiveWithArgs), appliedDirective.DirectiveType);
            CollectionAssert.AreEqual(new object[] { 23, "enum arg" }, appliedDirective.ArgumentValues);
        }

        [Test]
        public void DirectivesAreTransferedToGraphType()
        {
            var schema = new GraphSchema();

            var maker = new EnumGraphTypeMaker(schema);
            var graphType = maker.CreateGraphType(typeof(EnumValueWithDirective)).GraphType as IEnumGraphType;

            Assert.AreEqual(0, graphType.AppliedDirectives.Count);

            var value1 = graphType.Values["VALUE1"];
            var value2 = graphType.Values["VALUE2"];

            Assert.AreEqual(0, value1.AppliedDirectives.Count);
            Assert.AreEqual(1, value2.AppliedDirectives.Count);

            var appliedDirective = value2.AppliedDirectives.FirstOrDefault();
            Assert.IsNotNull(appliedDirective);
            Assert.AreEqual(value2, value2.AppliedDirectives.Parent);
            Assert.AreEqual(typeof(DirectiveWithArgs), appliedDirective.DirectiveType);
            CollectionAssert.AreEqual(new object[] { 33, "enum value arg" }, appliedDirective.ArgumentValues);
        }

        [TestCase(typeof(EnumWithValueOfNullKeyword), "NULL")]
        [TestCase(typeof(EnumWithValueOfTrueKeyword), "TRUE")]
        [TestCase(typeof(EnumWithValueOfFalseKeyword), "FALSE")]
        public void EnumValueIsKeyWord_ButFormattingChangesIt_WorksAsExpected(Type enumType, string enumValue)
        {
            var schema = new GraphSchema();

            var maker = new EnumGraphTypeMaker(schema);

            var graphType = maker.CreateGraphType(enumType).GraphType as IEnumGraphType;

            var value1 = graphType.Values[enumValue];
            Assert.IsNotNull(value1);
        }

        [TestCase(typeof(EnumWithValueOfNullKeyword), "NULL")]
        [TestCase(typeof(EnumWithValueOfTrueKeyword), "TRUE")]
        [TestCase(typeof(EnumWithValueOfFalseKeyword), "FALSE")]
        public void EnumValueIsKeyWord_AndFormattingMatchesKeyWord_ThrowsException(Type enumType, string enumValue)
        {
            var schema = new TestServerBuilder().AddGraphQL(o =>
            {
                o.DeclarationOptions.GraphNamingFormatter = new GraphNameFormatter(GraphNameFormatStrategy.LowerCase);
            })
            .Build()
            .Schema;

            var maker = new EnumGraphTypeMaker(schema);

            try
            {
                var graphType = maker.CreateGraphType(enumType).GraphType as IEnumGraphType;
            }
            catch (GraphTypeDeclarationException)
            {
                return;
            }

            Assert.Fail($"Expected {nameof(GraphTypeDeclarationException)} exception");
        }
    }
}