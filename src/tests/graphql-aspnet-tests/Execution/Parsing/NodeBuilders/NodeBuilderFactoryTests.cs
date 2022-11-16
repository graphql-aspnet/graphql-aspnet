// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.Parsing.NodeBuilders
{
    using System;
    using GraphQL.AspNet.Execution.Parsing.NodeBuilders;
    using GraphQL.AspNet.Execution.Parsing.SyntaxNodes;
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