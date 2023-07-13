// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Engine.TypeMakers
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
        [TestCase(typeof(int), GraphNameFormatStrategy.UpperCase, "Int")]
        [TestCase(typeof(int), GraphNameFormatStrategy.LowerCase, "Int")]
        [TestCase(typeof(int), GraphNameFormatStrategy.ProperCase, "Int")]
        [TestCase(typeof(float), GraphNameFormatStrategy.UpperCase, "Float")]
        [TestCase(typeof(float), GraphNameFormatStrategy.LowerCase, "Float")]
        [TestCase(typeof(float), GraphNameFormatStrategy.ProperCase, "Float")]
        [TestCase(typeof(string), GraphNameFormatStrategy.UpperCase, "String")]
        [TestCase(typeof(string), GraphNameFormatStrategy.LowerCase, "String")]
        [TestCase(typeof(string), GraphNameFormatStrategy.ProperCase, "String")]
        [TestCase(typeof(bool), GraphNameFormatStrategy.UpperCase, "Boolean")]
        [TestCase(typeof(bool), GraphNameFormatStrategy.LowerCase, "Boolean")]
        [TestCase(typeof(bool), GraphNameFormatStrategy.ProperCase, "Boolean")]
        [TestCase(typeof(GraphId), GraphNameFormatStrategy.UpperCase, "ID")]
        [TestCase(typeof(GraphId), GraphNameFormatStrategy.LowerCase, "ID")]
        [TestCase(typeof(GraphId), GraphNameFormatStrategy.ProperCase, "ID")]

        // non-fixed scalars will rename themselves
        [TestCase(typeof(decimal), GraphNameFormatStrategy.UpperCase, "DECIMAL")]
        [TestCase(typeof(decimal), GraphNameFormatStrategy.LowerCase, "decimal")]
        [TestCase(typeof(decimal), GraphNameFormatStrategy.ProperCase, "Decimal")]
        [TestCase(typeof(Uri), GraphNameFormatStrategy.UpperCase, "URI")]
        [TestCase(typeof(Uri), GraphNameFormatStrategy.LowerCase, "uri")]
        [TestCase(typeof(Uri), GraphNameFormatStrategy.ProperCase, "Uri")]
        public void BuiltInScalar_ObeysNamingRulesOfConfig(Type builtInScalarType, GraphNameFormatStrategy strategy, string expectedName)
        {
            var server = new TestServerBuilder()
                .AddGraphQL(o =>
                {
                    o.DeclarationOptions.GraphNamingFormatter
                        = new GraphNameFormatter(strategy);
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