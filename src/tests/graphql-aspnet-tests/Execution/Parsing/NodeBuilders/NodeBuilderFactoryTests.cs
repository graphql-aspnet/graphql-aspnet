// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Parsing2.NodeBuilders
{
    using System;
    using GraphQL.AspNet.Parsing2;
    using GraphQL.AspNet.Parsing2.NodeBuilders;
    using NUnit.Framework;

    [TestFixture]
    public class NodeBuilderFactoryTests
    {
        [Test]
        public void InvalidNodeType_ThrowsException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var builder = NodeBuilderFactory.CreateBuilder(SyntaxNodeType.Empty);
            });
        }
    }
}