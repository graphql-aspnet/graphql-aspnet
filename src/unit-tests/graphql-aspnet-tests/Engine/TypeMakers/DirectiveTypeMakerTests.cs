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
    using System.Linq;
    using GraphQL.AspNet.Engine;
    using GraphQL.AspNet.Execution.Resolvers;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.Generation;
    using GraphQL.AspNet.Schemas.Generation.TypeMakers;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Common.CommonHelpers;
    using GraphQL.AspNet.Tests.CommonHelpers;
    using GraphQL.AspNet.Tests.Engine.TypeMakers.TestData;
    using GraphQL.AspNet.Tests.Framework;
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

            var template = factory.MakeTemplate(typeof(MultiMethodDirective), TypeKind.DIRECTIVE);
            var typeMaker = new DirectiveMaker(server.Schema.Configuration, factory.CreateArgumentMaker());

            var directive = typeMaker.CreateGraphType(template).GraphType as IDirective;

            Assert.AreEqual("multiMethod", directive.Name);
            Assert.AreEqual("A Multi Method Directive", directive.Description);
            Assert.AreEqual(TypeKind.DIRECTIVE, directive.Kind);
            Assert.IsTrue((bool)directive.Publish);
            Assert.AreEqual(DirectiveLocation.FIELD | DirectiveLocation.SCALAR, directive.Locations);
            Assert.AreEqual(typeof(GraphDirectiveActionResolver), directive.Resolver.GetType());

            Assert.AreEqual(2, directive.Arguments.Count);

            var arg0 = Enumerable.FirstOrDefault(directive.Arguments);
            var arg1 = Enumerable.Skip(directive.Arguments, 1).FirstOrDefault();

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

            var factory = server.CreateMakerFactory();

            var template = factory.MakeTemplate(typeof(RepeatableDirective), TypeKind.DIRECTIVE);
            var typeMaker = new DirectiveMaker(server.Schema.Configuration, factory.CreateArgumentMaker());

            var directive = typeMaker.CreateGraphType(template).GraphType as IDirective;

            Assert.IsTrue((bool)directive.IsRepeatable);
            Assert.AreEqual("repeatable", directive.Name);
            Assert.AreEqual(TypeKind.DIRECTIVE, directive.Kind);
            Assert.IsTrue((bool)directive.Publish);
            Assert.AreEqual(DirectiveLocation.SCALAR, directive.Locations);

            Assert.AreEqual(2, directive.Arguments.Count);

            var arg0 = Enumerable.FirstOrDefault(directive.Arguments);
            var arg1 = Enumerable.Skip(directive.Arguments, 1).FirstOrDefault();

            Assert.IsNotNull(arg0);
            Assert.AreEqual("firstArg", arg0.Name);
            Assert.AreEqual(typeof(int), arg0.ObjectType);

            Assert.IsNotNull(arg1);
            Assert.AreEqual("secondArg", arg1.Name);
            Assert.AreEqual(typeof(TwoPropertyObject), arg1.ObjectType);
        }
    }
}