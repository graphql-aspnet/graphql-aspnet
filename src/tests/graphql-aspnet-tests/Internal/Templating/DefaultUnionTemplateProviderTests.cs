// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Internal.Templating
{
    using System;
    using System.Collections.Generic;
    using GraphQL.AspNet.Engine;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using NUnit.Framework;

    [TestFixture]
    public class DefaultUnionTemplateProviderTests
    {
        public class FakeProxy : IGraphUnionProxy
        {
            public string Name
            {
                get => throw new NotImplementedException();
                set => throw new NotImplementedException();
            }

            public string Description
            {
                get => throw new NotImplementedException();
                set => throw new NotImplementedException();
            }

            public HashSet<Type> Types => throw new NotImplementedException();

            public bool Publish
            {
                get => throw new NotImplementedException();
                set => throw new NotImplementedException();
            }

            public Type MapType(Type runtimeObjectType) => throw new NotImplementedException();
        }

        [Test]
        public void ParseAKnownScalar_ThrowsException()
        {
            var templateProvider = new DefaultTypeTemplateProvider();
            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                var template = templateProvider.ParseType(typeof(int), TypeKind.SCALAR);
            });
        }

        [Test]
        public void ParseAUnionProxy_ThrowsException()
        {
            var templateProvider = new DefaultTypeTemplateProvider();

            Assert.Throws<GraphTypeDeclarationException>(() =>
           {
               var template = templateProvider.ParseType(typeof(FakeProxy), TypeKind.UNION);
           });
        }
    }
}