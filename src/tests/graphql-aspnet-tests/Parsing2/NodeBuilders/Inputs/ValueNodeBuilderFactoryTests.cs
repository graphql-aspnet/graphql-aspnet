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
    using GraphQL.AspNet.Parsing2.Lexing;
    using GraphQL.AspNet.Parsing2.Lexing.Source;
    using GraphQL.AspNet.Parsing2.Lexing.Tokens;
    using GraphQL.AspNet.Parsing2.NodeBuilders.Inputs;
    using NUnit.Framework;

    [TestFixture]
    public class ValueNodeBuilderFactoryTests
    {
        [Test]
        public void InvalidToken_ReturnsNull()
        {
            var tokenStream = Lexer.Tokenize(new SourceText("}".AsSpan()));
            tokenStream.Prime();

            var builder = ValueNodeBuilderFactory.CreateBuilder(tokenStream);
            Assert.IsNull(builder);
        }
    }
}