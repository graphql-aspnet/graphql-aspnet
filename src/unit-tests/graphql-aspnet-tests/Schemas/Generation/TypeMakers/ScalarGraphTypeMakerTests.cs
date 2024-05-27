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
    using GraphQL.AspNet.Configuration.Formatting;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.Generation.TypeMakers;
    using GraphQL.AspNet.Schemas.Generation.TypeTemplates;
    using GraphQL.AspNet.Tests.Framework;
    using NUnit.Framework;

    [TestFixture]
    public class ScalarGraphTypeMakerTests
    {
        [Test]
        public void RegisteredScalarIsReturned()
        {
            var schema = new GraphSchema();
            var maker = new ScalarGraphTypeMaker(schema.Configuration);

            var result = maker.CreateGraphType(GraphQLTemplateHelper.CreateGraphTypeTemplate(typeof(int)));
            Assert.IsNotNull(result?.GraphType);
            Assert.AreEqual(typeof(int), result.ConcreteType);
        }

        [Test]
        public void NullType_ReturnsNullResult()
        {
            var schema = new GraphSchema();
            var maker = new ScalarGraphTypeMaker(schema.Configuration);

            var result = maker.CreateGraphType(null);
            Assert.IsNull(result);
        }

        // fixed name scalars will never be renamed
        [TestCase(typeof(int), TextFormatOptions.UpperCase, "Int")]
        [TestCase(typeof(int), TextFormatOptions.LowerCase, "Int")]
        [TestCase(typeof(int), TextFormatOptions.ProperCase, "Int")]
        [TestCase(typeof(float), TextFormatOptions.UpperCase, "Float")]
        [TestCase(typeof(float), TextFormatOptions.LowerCase, "Float")]
        [TestCase(typeof(float), TextFormatOptions.ProperCase, "Float")]
        [TestCase(typeof(string), TextFormatOptions.UpperCase, "String")]
        [TestCase(typeof(string), TextFormatOptions.LowerCase, "String")]
        [TestCase(typeof(string), TextFormatOptions.ProperCase, "String")]
        [TestCase(typeof(bool), TextFormatOptions.UpperCase, "Boolean")]
        [TestCase(typeof(bool), TextFormatOptions.LowerCase, "Boolean")]
        [TestCase(typeof(bool), TextFormatOptions.ProperCase, "Boolean")]
        [TestCase(typeof(GraphId), TextFormatOptions.UpperCase, "ID")]
        [TestCase(typeof(GraphId), TextFormatOptions.LowerCase, "ID")]
        [TestCase(typeof(GraphId), TextFormatOptions.ProperCase, "ID")]

        // non-fixed scalars will rename themselves
        [TestCase(typeof(decimal), TextFormatOptions.UpperCase, "DECIMAL")]
        [TestCase(typeof(decimal), TextFormatOptions.LowerCase, "decimal")]
        [TestCase(typeof(decimal), TextFormatOptions.ProperCase, "Decimal")]
        [TestCase(typeof(Uri), TextFormatOptions.UpperCase, "URI")]
        [TestCase(typeof(Uri), TextFormatOptions.LowerCase, "uri")]
        [TestCase(typeof(Uri), TextFormatOptions.ProperCase, "Uri")]
        public void BuiltInScalar_ObeysNamingRulesOfConfig(Type builtInScalarType, TextFormatOptions nameFormat, string expectedName)
        {
            var server = new TestServerBuilder()
                .AddGraphQL(o =>
                {
                    o.DeclarationOptions.SchemaFormatStrategy = SchemaFormatStrategyBuilder
                            .Create(nameFormat, applyDefaultRules: false)
                            .Build();
                })
                .Build();

            var maker = new ScalarGraphTypeMaker(server.Schema.Configuration);

            var template = new ScalarGraphTypeTemplate(builtInScalarType);
            template.Parse();
            template.ValidateOrThrow();

            var result = maker.CreateGraphType(template);
            Assert.AreEqual(expectedName, result.GraphType.Name);
        }
    }
}