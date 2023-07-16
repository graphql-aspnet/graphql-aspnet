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
    using GraphQL.AspNet.Execution.Resolvers;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.Generation.TypeMakers;
    using GraphQL.AspNet.Schemas.Generation.TypeTemplates;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Common.CommonHelpers;
    using GraphQL.AspNet.Tests.CommonHelpers;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Schemas.Generation.TypeMakers.TestData;
    using NUnit.Framework;

    [TestFixture]
    public class DirectiveTypeMakerTests : GraphTypeMakerTestBase
    {
        [Test]
        public void Directive_BasicPropertyCheck()
        {
            var builder = new TestServerBuilder();
            var server = builder.Build();

            var factory = server.CreateMakerFactory();

            var template = new GraphDirectiveTemplate(typeof(MultiMethodDirective));
            template.Parse();
            template.ValidateOrThrow();

            var typeMaker = new DirectiveMaker(server.Schema, new GraphArgumentMaker(server.Schema));

            var directive = typeMaker.CreateGraphType(template).GraphType as IDirective;

            Assert.AreEqual("multiMethod", directive.Name);
            Assert.AreEqual("A Multi Method Directive", directive.Description);
            Assert.AreEqual(TypeKind.DIRECTIVE, directive.Kind);
            Assert.IsTrue(directive.Publish);
            Assert.AreEqual(DirectiveLocation.FIELD | DirectiveLocation.SCALAR, directive.Locations);
            Assert.AreEqual(typeof(GraphDirectiveActionResolver), directive.Resolver.GetType());

            Assert.AreEqual(2, directive.Arguments.Count);

            var arg0 = directive.Arguments.FirstOrDefault();
            var arg1 = directive.Arguments.Skip(1).FirstOrDefault();

            Assert.IsNotNull(arg0);
            Assert.AreEqual("firstArg", arg0.Name);
            Assert.AreEqual(typeof(int), arg0.ObjectType);

            Assert.IsNotNull(arg1);
            Assert.AreEqual("secondArg", arg1.Name);
            Assert.AreEqual(typeof(TwoPropertyObject), arg1.ObjectType);
        }

        [Test]
        public void Directive_RepeatableAttributeIsSetWhenPresent()
        {
            var builder = new TestServerBuilder();
            var server = builder.Build();

            var template = new GraphDirectiveTemplate(typeof(RepeatableDirective));
            template.Parse();
            template.ValidateOrThrow();

            var typeMaker = new DirectiveMaker(server.Schema, new GraphArgumentMaker(server.Schema));

            var directive = typeMaker.CreateGraphType(template).GraphType as IDirective;

            Assert.IsTrue(directive.IsRepeatable);
            Assert.AreEqual("repeatable", directive.Name);
            Assert.AreEqual(TypeKind.DIRECTIVE, directive.Kind);
            Assert.IsTrue(directive.Publish);
            Assert.AreEqual(DirectiveLocation.SCALAR, directive.Locations);

            Assert.AreEqual(2, directive.Arguments.Count);

            var arg0 = directive.Arguments.FirstOrDefault();
            var arg1 = directive.Arguments.Skip(1).FirstOrDefault();

            Assert.IsNotNull(arg0);
            Assert.AreEqual("firstArg", arg0.Name);
            Assert.AreEqual(typeof(int), arg0.ObjectType);

            Assert.IsNotNull(arg1);
            Assert.AreEqual("secondArg", arg1.Name);
            Assert.AreEqual(typeof(TwoPropertyObject), arg1.ObjectType);
        }

        [TestCase(
            SchemaArgumentBindingRules.ParametersPreferQueryResolution,
            typeof(ArgCheckImplicitSchemaItemDirective),
            true)]
        [TestCase(
            SchemaArgumentBindingRules.ParametersPreferQueryResolution,
            typeof(ArgCheckImplicitInjectedItemDirective),
            false)]
        [TestCase(
            SchemaArgumentBindingRules.ParametersPreferQueryResolution,
            typeof(ArgCheckExplicitValidSchemaItemDirective),
            true)]
        [TestCase(
            SchemaArgumentBindingRules.ParametersPreferQueryResolution,
            typeof(ArgCheckExplicitInvalidSchemaItemDirective),
            true)]
        [TestCase(
            SchemaArgumentBindingRules.ParametersPreferQueryResolution,
            typeof(ArgCheckExplicitInjectedItemDirective),
            false)]

        [TestCase(
            SchemaArgumentBindingRules.ParametersRequireFromServicesDeclaration,
            typeof(ArgCheckImplicitSchemaItemDirective),
            true)]
        [TestCase(
            SchemaArgumentBindingRules.ParametersRequireFromServicesDeclaration,
            typeof(ArgCheckImplicitInjectedItemDirective),
            true)]
        [TestCase(
            SchemaArgumentBindingRules.ParametersRequireFromServicesDeclaration,
            typeof(ArgCheckExplicitValidSchemaItemDirective),
            true)]
        [TestCase(
            SchemaArgumentBindingRules.ParametersRequireFromServicesDeclaration,
            typeof(ArgCheckExplicitInvalidSchemaItemDirective),
            true)]
        [TestCase(
            SchemaArgumentBindingRules.ParametersRequireFromServicesDeclaration,
            typeof(ArgCheckExplicitInjectedItemDirective),
            false)]

        [TestCase(
            SchemaArgumentBindingRules.ParametersRequireFromGraphQLDeclaration,
            typeof(ArgCheckImplicitSchemaItemDirective),
            false)]
        [TestCase(
            SchemaArgumentBindingRules.ParametersRequireFromGraphQLDeclaration,
            typeof(ArgCheckImplicitInjectedItemDirective),
            false)]
        [TestCase(
            SchemaArgumentBindingRules.ParametersRequireFromGraphQLDeclaration,
            typeof(ArgCheckExplicitValidSchemaItemDirective),
            true)]
        [TestCase(
            SchemaArgumentBindingRules.ParametersRequireFromGraphQLDeclaration,
            typeof(ArgCheckExplicitInvalidSchemaItemDirective),
            true)]
        [TestCase(
            SchemaArgumentBindingRules.ParametersRequireFromGraphQLDeclaration,
            typeof(ArgCheckExplicitInjectedItemDirective),
            false)]
        public void ArgInclusionCheck(SchemaArgumentBindingRules bindingRule, Type directiveType, bool shouldBeIncluded)
        {
            var server = new TestServerBuilder()
                .AddGraphQL(o =>
                {
                    o.DeclarationOptions.ArgumentBindingRule = bindingRule;
                })
                .Build();

            var template = new GraphDirectiveTemplate(directiveType);
            template.Parse();
            template.ValidateOrThrow();

            var typeMaker = new DirectiveMaker(server.Schema, new GraphArgumentMaker(server.Schema));

            var directive = typeMaker.CreateGraphType(template).GraphType as IDirective;

            Assert.AreEqual(TypeKind.DIRECTIVE, directive.Kind);

            if (shouldBeIncluded)
                Assert.AreEqual(1, directive.Arguments.Count);
            else
                Assert.AreEqual(0, directive.Arguments.Count);
        }
    }
}