// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Schemas.Generation.TypeMakers
{
    using System;
    using System.Linq;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Configuration.Formatting;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.Generation.TypeMakers;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Schemas.Generation.TypeMakers.TestData;
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

            var maker = new EnumGraphTypeMaker(schema.Configuration);
            var template = GraphQLTemplateHelper.CreateEnumTemplate<EnumWithUndeclaredValues>();

            var graphType = maker.CreateGraphType(template).GraphType as IEnumGraphType;
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

            var maker = new EnumGraphTypeMaker(schema.Configuration);
            var template = GraphQLTemplateHelper.CreateEnumTemplate<EnumWithUndeclaredValues>();
            var graphType = maker.CreateGraphType(template).GraphType as IEnumGraphType;

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

            var maker = new EnumGraphTypeMaker(schema.Configuration);
            var template = GraphQLTemplateHelper.CreateEnumTemplate<EnumWithGraphName>();

            var graphType = maker.CreateGraphType(template).GraphType as IEnumGraphType;
            Assert.AreEqual("ValidGraphName", graphType.Name);
        }

        [Test]
        public void CreateGraphType_ParsesAsExpected()
        {
            var schema = new TestServerBuilder()
                .AddGraphQL(o =>
                {
                    o.DeclarationOptions.SchemaFormatStrategy =
                        new GraphSchemaFormatStrategy(enumValueStrategy: GraphNameFormatStrategy.NoChanges);
                })
                .Build()
                .Schema;

            var template = GraphQLTemplateHelper.CreateEnumTemplate<EnumWithDescriptionOnValues>();

            var maker = new EnumGraphTypeMaker(schema.Configuration);
            var graphType = maker.CreateGraphType(template).GraphType as IEnumGraphType;

            Assert.IsNotNull(graphType);
            Assert.AreEqual(template.Name, graphType.Name);

            Assert.IsTrue(graphType is EnumGraphType);
            Assert.AreEqual(4, ((EnumGraphType)graphType).Values.Count);
        }

        [Test]
        public void AppliedDirectives_TransferFromTemplate()
        {
            var schema = new GraphSchema();

            var template = GraphQLTemplateHelper.CreateEnumTemplate<EnumWithDirective>();
            var maker = new EnumGraphTypeMaker(schema.Configuration);

            var graphType = maker.CreateGraphType(template).GraphType as IEnumGraphType;

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

            var maker = new EnumGraphTypeMaker(schema.Configuration);
            var template = GraphQLTemplateHelper.CreateEnumTemplate<EnumValueWithDirective>();

            var graphType = maker.CreateGraphType(template).GraphType as IEnumGraphType;

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
        public void EnumValueIsKeyword_ButFormattingDoesNotMatchKeyword_WorksAsExpected(Type enumType, string enumValue)
        {
            var server = new TestServerBuilder()
                .Build();

            var maker = new EnumGraphTypeMaker(server.Schema.Configuration);
            var template = GraphQLTemplateHelper.CreateGraphTypeTemplate(enumType, TypeKind.ENUM) as IGraphTypeTemplate;

            var graphType = maker.CreateGraphType(template).GraphType as IEnumGraphType;

            var value1 = graphType.Values[enumValue];
            Assert.IsNotNull(value1);
        }

        [TestCase(typeof(EnumWithValueOfNullKeyword), "NULL")]
        [TestCase(typeof(EnumWithValueOfTrueKeyword), "TRUE")]
        [TestCase(typeof(EnumWithValueOfFalseKeyword), "FALSE")]
        public void EnumValueIsKeyword_AndFormattingMatchesKeyword_ThrowsException(Type enumType, string enumValue)
        {
            var schema = new TestServerBuilder().AddGraphQL(o =>
            {
                o.DeclarationOptions.SchemaFormatStrategy = new GraphSchemaFormatStrategy(GraphNameFormatStrategy.LowerCase);
            })
            .Build()
            .Schema;

            var maker = new EnumGraphTypeMaker(schema.Configuration);
            var template = GraphQLTemplateHelper.CreateGraphTypeTemplate(enumType, TypeKind.ENUM) as IGraphTypeTemplate;

            try
            {
                var graphType = maker.CreateGraphType(template).GraphType as IEnumGraphType;
            }
            catch (GraphTypeDeclarationException)
            {
                return;
            }

            Assert.Fail($"Expected {nameof(GraphTypeDeclarationException)} exception");
        }

        [Test]
        public void EnumType_InternalNameCheck()
        {
            var schema = new GraphSchema();

            var maker = new EnumGraphTypeMaker(schema.Configuration);
            var template = GraphQLTemplateHelper.CreateGraphTypeTemplate(typeof(EnumWithInternalNames), TypeKind.ENUM) as IGraphTypeTemplate;

            var graphType = maker.CreateGraphType(template).GraphType as IEnumGraphType;

            Assert.AreEqual("EnumInternalName", graphType.InternalName);
        }

        [Test]
        public void EnumValue_InternalNameCheck()
        {
            var schema = new GraphSchema();

            var maker = new EnumGraphTypeMaker(schema.Configuration);
            var template = GraphQLTemplateHelper.CreateGraphTypeTemplate(typeof(EnumWithInternalNames), TypeKind.ENUM) as IGraphTypeTemplate;

            var graphType = maker.CreateGraphType(template).GraphType as IEnumGraphType;
            var value = graphType.Values.Single().Value;

            Assert.AreEqual("Value1InternalName", value.InternalName);
        }
    }
}