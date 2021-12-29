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
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Configuration.Formatting;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Default.TypeMakers.TestData;
    using GraphQL.AspNet.Tests.Framework;
    using NUnit.Framework;

    [TestFixture]
    public class EnumGraphTypeMakerTests : GraphTypeMakerTestBase
    {
        [Test]
        public void Parse_EnumWithUndeclaredValues_WhenConfigRequiresDeclaration_DoesntIncludeUndeclared_InGraphType()
        {
            var template = TemplateHelper.CreateEnumTemplate<EnumWithUndeclaredValues>();

            var builder = new TestServerBuilder();
            builder.AddGraphQL(o =>
            {
                o.DeclarationOptions.FieldDeclarationRequirements = TemplateDeclarationRequirements.EnumValue;
            });

            var server = builder.Build();

            var graphType = server.CreateGraphType(template.ObjectType, TypeKind.ENUM).GraphType as IEnumGraphType;
            Assert.AreEqual(2, graphType.Values.Count);
            Assert.IsTrue(graphType.Values.ContainsKey("DECLAREDVALUE1"));
            Assert.IsTrue(graphType.Values.ContainsKey("VALUE_AWESOME"));
        }

        [Test]
        public void Parse_EnumWithUndeclaredValues_WhenConfigDoesNotRequireDeclaration_DoesIncludeUndeclared_InGraphType()
        {
            var builder = new TestServerBuilder()
                .AddGraphType<EnumWithUndeclaredValues>();
            builder.AddGraphQL(o =>
            {
                o.DeclarationOptions.FieldDeclarationRequirements = TemplateDeclarationRequirements.None;
            });

            var server = builder.Build();
            var graphType = server.CreateGraphType(typeof(EnumWithUndeclaredValues), TypeKind.ENUM).GraphType as IEnumGraphType;
            Assert.AreEqual(3, graphType.Values.Count);
            Assert.IsTrue(graphType.Values.ContainsKey("DECLAREDVALUE1"));

            Assert.IsTrue(graphType.Values.ContainsKey("VALUE_AWESOME"));
            Assert.IsFalse(graphType.Values.ContainsKey("DECLAREDVALUE2"));

            Assert.IsTrue(graphType.Values.ContainsKey("UNDECLAREDVALUE1"));
        }

        [Test]
        public void Parse_EnumWithCustomGraphTypeName_YieldsName_InGraphType()
        {
            var template = TemplateHelper.CreateEnumTemplate<EnumWithGraphName>();

            var builder = new TestServerBuilder()
                .AddGraphQL(o =>
                {
                    o.AddGraphType<EnumWithGraphName>();
                });

            var server = builder.Build();
            var graphType = server.CreateGraphType(template.ObjectType, TypeKind.ENUM).GraphType as IEnumGraphType;

            Assert.AreEqual("ValidGraphName", graphType.Name);
        }

        [Test]
        public void CreateGraphType_ParsesAsExpected()
        {
            var builder = new TestServerBuilder();
            builder.AddGraphQL(o =>
            {
                o.DeclarationOptions.GraphNamingFormatter =
                    new GraphNameFormatter(enumValueStrategy: GraphNameFormatStrategy.NoChanges);
            });

            var server = builder.Build();
            var template = TemplateHelper.CreateEnumTemplate<EnumWithDescriptionOnValues>();

            var type = server.CreateGraphType(typeof(EnumWithDescriptionOnValues), TypeKind.ENUM).GraphType as IEnumGraphType;
            Assert.IsNotNull(type);
            Assert.AreEqual(template.Name, type.Name);

            Assert.IsTrue(type is EnumGraphType);
            Assert.AreEqual(4, ((EnumGraphType)type).Values.Count);
        }
    }
}