﻿// *************************************************************
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
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.Generation.TypeMakers;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Schemas.Generation.TypeMakers.TestData;
    using NUnit.Framework;

    [TestFixture]
    public class UnionTypeMakerTests : GraphTypeMakerTestBase
    {
        [Test]
        public void ActionTemplate_CreateUnionType_PropertyCheck()
        {
            var schema = new GraphSchema();
            schema.SetNoAlterationConfiguration();

            var action = GraphQLTemplateHelper.CreateActionMethodTemplate<UnionTestController>(nameof(UnionTestController.TwoTypeUnion));
            var unionResult = new UnionGraphTypeMaker(schema.Configuration).CreateUnionFromProxy(action.UnionProxy);
            var union = unionResult.GraphType as IUnionGraphType;

            Assert.IsNotNull(union);
            Assert.IsTrue(union is UnionGraphType);
            Assert.AreEqual("FragmentData", union.Name);
            Assert.IsNull(union.Description);

            Assert.AreEqual(2, union.PossibleGraphTypeNames.Count());
            Assert.AreEqual(2, union.PossibleConcreteTypes.Count());

            Assert.IsTrue(union.PossibleGraphTypeNames.Contains(nameof(UnionDataA)));
            Assert.IsTrue(union.PossibleGraphTypeNames.Contains(nameof(UnionDataB)));
            Assert.IsTrue(union.PossibleConcreteTypes.Contains(typeof(UnionDataA)));
            Assert.IsTrue(union.PossibleConcreteTypes.Contains(typeof(UnionDataB)));
        }

        [Test]
        public void UnionProxyWithDirectives_DirectivesAreApplied()
        {
            var schema = new GraphSchema();
            schema.SetNoAlterationConfiguration();

            var maker = new UnionGraphTypeMaker(schema.Configuration);
            var unionResult = maker.CreateUnionFromProxy(new UnionProxyWithDirective());
            var unionType = unionResult.GraphType as IUnionGraphType;

            Assert.IsNotNull(unionType);
            Assert.AreEqual(1, unionType.AppliedDirectives.Count);
            Assert.AreEqual(unionType, unionType.AppliedDirectives.Parent);

            var appliedDirective = unionType.AppliedDirectives.FirstOrDefault();
            Assert.IsNotNull(appliedDirective);
            Assert.AreEqual(typeof(DirectiveWithArgs), appliedDirective.DirectiveType);
            CollectionAssert.AreEqual(new object[] { 121, "union directive" }, appliedDirective.ArgumentValues);
        }

        [Test]
        public void UnionProxyWithDirectives_InvalidDirectiveType_ThrowsException()
        {
            var schema = new GraphSchema();
            schema.SetNoAlterationConfiguration();
            var maker = new UnionGraphTypeMaker(schema.Configuration);

            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                var unionType = maker.CreateUnionFromProxy(new UnionProxyWithInvalidDirective());
            });
        }

        [Test]
        public void NullProxy_YieldsNullGraphType()
        {
            var schema = new GraphSchema();
            schema.SetNoAlterationConfiguration();
            var maker = new UnionGraphTypeMaker(schema.Configuration);

            var unionType = maker.CreateUnionFromProxy(null);
            Assert.IsNull(unionType);
        }

        [Test]
        public void Proxy_WithCustomInternalName_IsSetToSaidName()
        {
            var schema = new GraphSchema();
            schema.SetNoAlterationConfiguration();
            var maker = new UnionGraphTypeMaker(schema.Configuration);

            var unionType = maker.CreateUnionFromProxy(new UnionWithInternalName()).GraphType as IUnionGraphType;
            Assert.AreEqual("TestUnionInternalName", unionType.InternalName);
        }

        [Test]
        public void Proxy_WithNoInternalName_IsSetToProxyName()
        {
            var schema = new GraphSchema();
            schema.SetNoAlterationConfiguration();
            var maker = new UnionGraphTypeMaker(schema.Configuration);

            var unionType = maker.CreateUnionFromProxy(new UnionWithNoInternalName()).GraphType as IUnionGraphType;
            Assert.AreEqual("UnionWithNoInternalName", unionType.InternalName);
        }
    }
}