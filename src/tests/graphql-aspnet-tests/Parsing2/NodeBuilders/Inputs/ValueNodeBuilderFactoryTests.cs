// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Parsing2.NodeBuilders.Inputs
{
    using System;
    using GraphQL.AspNet.Parsing.Lexing.Tokens;
    using GraphQL.AspNet.Parsing2.NodeBuilders.Inputs;
    using NUnit.Framework;

    [TestFixture]
    public class ValueNodeBuilderFactoryTests
    {
        [Test]
        public void InvalidToken_ReturnsNull()
        {
            var builder = ValueNodeBuilderFactory.CreateBuilder(new LexicalToken(TokenType.CurlyBraceRight));
            Assert.IsNull(builder);
        }
    }
}