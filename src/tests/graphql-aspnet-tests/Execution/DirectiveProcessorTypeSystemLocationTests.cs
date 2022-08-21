// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution
{
    using System;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Execution.DirectiveProcessorTypeSystemLocationTestData;
    using GraphQL.AspNet.Tests.Framework;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;

    [TestFixture]
    public class DirectiveProcessorTypeSystemLocationTests
    {
        [TestCase(typeof(ObjectTestObject), TypeKind.OBJECT, DirectiveLocation.OBJECT)]
        [TestCase(typeof(ObjectTestObject), TypeKind.INPUT_OBJECT, DirectiveLocation.INPUT_OBJECT)]
        [TestCase(typeof(FieldTestObject), TypeKind.OBJECT, DirectiveLocation.FIELD_DEFINITION)]
        [TestCase(typeof(FieldTestObject), TypeKind.INPUT_OBJECT, DirectiveLocation.INPUT_FIELD_DEFINITION)]
        [TestCase(typeof(InterfaceTestObject), TypeKind.INTERFACE, DirectiveLocation.INTERFACE)]
        [TestCase(typeof(EnumTestObject), TypeKind.ENUM, DirectiveLocation.ENUM)]
        [TestCase(typeof(EnumValueTestObject), TypeKind.ENUM, DirectiveLocation.ENUM_VALUE)]
        [TestCase(typeof(ArgumentDefinitionTestObject), TypeKind.OBJECT, DirectiveLocation.ARGUMENT_DEFINITION)]
        [TestCase(typeof(UniontTestObject), TypeKind.UNION, DirectiveLocation.UNION)]
        public void Execute(Type type, TypeKind registeredAs, DirectiveLocation expectedLocation)
        {
            var directiveInstance = new LocationTestDirective();

            var builder = new TestServerBuilder();
            builder.AddType(type, registeredAs);
            builder.AddSingleton(directiveInstance);
            builder.AddDirective<LocationTestDirective>();

            // execute
            var server = builder.Build();

            Assert.AreEqual(expectedLocation, directiveInstance.ExecutedLocation);
        }

        [Test]
        public void ApplyDirectiveToScalar()
        {
            var directiveInstance = new LocationTestDirective();

            var builder = new TestServerBuilder<TestSchema>();
            builder.AddSingleton(directiveInstance);
            builder.AddType<int>();
            builder.AddDirective<LocationTestDirective>();
            builder.AddGraphQL(o =>
            {
                o.ApplyDirective<LocationTestDirective>()
                    .ToItems(x => x is IScalarGraphType sgt && sgt.ObjectType == typeof(int));
            });

            // execute
            var server = builder.Build();

            Assert.AreEqual(DirectiveLocation.SCALAR, directiveInstance.ExecutedLocation);
        }

        [Test]
        public void ApplyDirectiveToSchema()
        {
            var directiveInstance = new LocationTestDirective();

            var builder = new TestServerBuilder<TestSchema>();
            builder.AddSingleton(directiveInstance);
            builder.AddDirective<LocationTestDirective>();

            // execute
            var server = builder.Build();

            Assert.AreEqual(DirectiveLocation.SCHEMA, directiveInstance.ExecutedLocation);
        }
    }
}
