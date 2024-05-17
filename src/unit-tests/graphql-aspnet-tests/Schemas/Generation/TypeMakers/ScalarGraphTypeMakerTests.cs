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
        [TestCase(typeof(int), SchemaItemNameFormatOptions.UpperCase, "Int")]
        [TestCase(typeof(int), SchemaItemNameFormatOptions.LowerCase, "Int")]
        [TestCase(typeof(int), SchemaItemNameFormatOptions.ProperCase, "Int")]
        [TestCase(typeof(float), SchemaItemNameFormatOptions.UpperCase, "Float")]
        [TestCase(typeof(float), SchemaItemNameFormatOptions.LowerCase, "Float")]
        [TestCase(typeof(float), SchemaItemNameFormatOptions.ProperCase, "Float")]
        [TestCase(typeof(string), SchemaItemNameFormatOptions.UpperCase, "String")]
        [TestCase(typeof(string), SchemaItemNameFormatOptions.LowerCase, "String")]
        [TestCase(typeof(string), SchemaItemNameFormatOptions.ProperCase, "String")]
        [TestCase(typeof(bool), SchemaItemNameFormatOptions.UpperCase, "Boolean")]
        [TestCase(typeof(bool), SchemaItemNameFormatOptions.LowerCase, "Boolean")]
        [TestCase(typeof(bool), SchemaItemNameFormatOptions.ProperCase, "Boolean")]
        [TestCase(typeof(GraphId), SchemaItemNameFormatOptions.UpperCase, "ID")]
        [TestCase(typeof(GraphId), SchemaItemNameFormatOptions.LowerCase, "ID")]
        [TestCase(typeof(GraphId), SchemaItemNameFormatOptions.ProperCase, "ID")]

        // non-fixed scalars will rename themselves
        [TestCase(typeof(decimal), SchemaItemNameFormatOptions.UpperCase, "DECIMAL")]
        [TestCase(typeof(decimal), SchemaItemNameFormatOptions.LowerCase, "decimal")]
        [TestCase(typeof(decimal), SchemaItemNameFormatOptions.ProperCase, "Decimal")]
        [TestCase(typeof(Uri), SchemaItemNameFormatOptions.UpperCase, "URI")]
        [TestCase(typeof(Uri), SchemaItemNameFormatOptions.LowerCase, "uri")]
        [TestCase(typeof(Uri), SchemaItemNameFormatOptions.ProperCase, "Uri")]
        public void BuiltInScalar_ObeysNamingRulesOfConfig(Type builtInScalarType, SchemaItemNameFormatOptions strategy, string expectedName)
        {
            var server = new TestServerBuilder()
                .AddGraphQL(o =>
                {
                    o.DeclarationOptions.SchemaFormatStrategy
                        = new SchemaFormatStrategy(strategy);
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