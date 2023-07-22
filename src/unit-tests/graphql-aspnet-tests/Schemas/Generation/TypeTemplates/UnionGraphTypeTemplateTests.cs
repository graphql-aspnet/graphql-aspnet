// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Schemas.Generation.TypeTemplates
{
    using System;
    using System.Security.Cryptography;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Schemas.Generation.TypeTemplates;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Common.CommonHelpers;
    using GraphQL.AspNet.Tests.Schemas.Generation.TypeTemplates.UnionTestData;
    using NUnit.Framework;

    [TestFixture]
    public class UnionGraphTypeTemplateTests
    {
        [Test]
        public void NotAProxy_ThrowsException()
        {
            var instance = new UnionGraphTypeTemplate(typeof(TwoPropertyObject));
            instance.Parse();

            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                instance.ValidateOrThrow();
            });
        }

        [Test]
        public void ValidateProxy_ParsesCorrectly()
        {
            var instance = new UnionGraphTypeTemplate(typeof(ValidTestUnion));

            instance.Parse();
            instance.ValidateOrThrow();

            Assert.AreEqual(TypeKind.UNION, instance.Kind);
            Assert.AreEqual("ValidUnion", instance.Name);
            Assert.AreEqual("My Union Desc", instance.Description);
            Assert.AreEqual("ValidUnion", instance.InternalName); // should use union name when supplied
            Assert.AreEqual(typeof(ValidTestUnion), instance.ProxyType);
            Assert.IsTrue(instance.Publish);
        }
    }
}