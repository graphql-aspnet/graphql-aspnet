﻿// *************************************************************
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
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Internal.Resolvers;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Engine.TypeMakers.TestData;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using NUnit.Framework;

    [TestFixture]
    public class DirectiveTypeMakerTests : GraphTypeMakerTestBase
    {
        [Test]
        public void Directive_BasicPropertyCheck()
        {
            var builder = new TestServerBuilder();
            var server = builder.Build();
            var typeMaker = new DefaultGraphTypeMakerProvider()
                                .CreateTypeMaker(server.Schema, TypeKind.DIRECTIVE);

            var directive = typeMaker.CreateGraphType(typeof(MultiMethodDirective)).GraphType as IDirective;

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
            var typeMaker = new DefaultGraphTypeMakerProvider()
                                .CreateTypeMaker(server.Schema, TypeKind.DIRECTIVE);

            var directive = typeMaker.CreateGraphType(typeof(RepeatableDirective)).GraphType as IDirective;

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