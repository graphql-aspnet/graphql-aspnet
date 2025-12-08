// *************************************************************
//  project:  graphql-aspnet
//  --
//  repo: https://github.com/graphql-aspnet
//  docs: https://graphql-aspnet.github.io
//  --
//  License:  MIT
//  *************************************************************

namespace GraphQL.AspNet.Tests.Schemas
{
    using System;
    using System.Linq;
    using GraphQL.AspNet.Directives.Global;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Schemas.SchemaTestData;
    using NUnit.Framework;

    [TestFixture]
    public class SchemaTypeOneOfTests
    {
        private IGraphType MakeGraphType(Type type, TypeKind kind)
        {
            var testServer = new TestServerBuilder().Build();

            switch (kind)
            {
                case TypeKind.UNION:
                    var proxy = GraphQLProviders.GraphTypeMakerProvider.CreateUnionProxyFromType(type);
                    var unionMaker = GraphQLProviders.GraphTypeMakerProvider.CreateUnionMaker(testServer.Schema);
                    return unionMaker.CreateUnionFromProxy(proxy).GraphType;

                default:
                    var maker = GraphQLProviders.GraphTypeMakerProvider.CreateTypeMaker(testServer.Schema, kind);
                    return maker.CreateGraphType(type).GraphType;
            }
        }

        [Test]
        public void InputUnion_WithOneOfAttribute_CorrectlyAssignsDirective_WhenCreated()
        {
            var graphType = this.MakeGraphType(typeof(InputUnionSimpleObject), TypeKind.INPUT_OBJECT) as IInputObjectGraphType;

            Assert.That(graphType.AppliedDirectives, Is.Not.Null);
            Assert.That(graphType.AppliedDirectives.Count, Is.EqualTo(1));
            Assert.That(graphType.AppliedDirectives.First().DirectiveType, Is.EqualTo(typeof(OneOfDirective)));
        }

        [Test]
        public void InputUnion_InheritingFromGraphUnion_CorrectlyAssignsDirective_WhenCreated()
        {
            var graphType = this.MakeGraphType(typeof(InputUnionFromGraphInputUnion), TypeKind.INPUT_OBJECT) as IInputObjectGraphType;

            Assert.That(graphType.AppliedDirectives, Is.Not.Null);
            Assert.That(graphType.AppliedDirectives.Count, Is.EqualTo(1));
            Assert.That(graphType.AppliedDirectives.First().DirectiveType, Is.EqualTo(typeof(OneOfDirective)));
        }

        [Test]
        public void InputObject_NotDeclaredAsOneOf_IsNotAssignedDirective_WhenCreated()
        {
            var graphType = this.MakeGraphType(typeof(CountryData), TypeKind.INPUT_OBJECT) as IInputObjectGraphType;
            Assert.That(graphType.AppliedDirectives, Is.Not.Null);
            Assert.That(graphType.AppliedDirectives.Count, Is.EqualTo(0));
        }
    }
}