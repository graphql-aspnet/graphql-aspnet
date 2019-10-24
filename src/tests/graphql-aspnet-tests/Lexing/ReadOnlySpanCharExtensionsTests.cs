// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Lexing
{
    using System;
    using GraphQL.AspNet.Parsing.Lexing.Source;
    using NUnit.Framework;

    [TestFixture]
    public class ReadOnlySpanCharExtensionsTests
    {
        [TestCase(@"\u1234", true)]
        [TestCase(@"\u12F4", true)]
        [TestCase(@"\u1", true)]
        [TestCase(@"\u12", true)]
        [TestCase(@"\u123", true)]
        [TestCase(@"\uFFFF", true)]
        [TestCase(@"\uFfDf", true)]
        [TestCase(@"\u0A1b", true)]
        [TestCase(@"\ufffg", false)]
        [TestCase(@"\ufGff", false)]
        [TestCase(@"\r0A1b", false)]
        [TestCase(@"\u", false)]
        [TestCase(@"\", false)]
        [TestCase(@"", false)]
        [TestCase("\\u12\rC", false)]
        [TestCase(@"\u12345", false)]
        [TestCase(@"\U1234", false)]
        [TestCase(@"abc", false)]
        public void ReadOnlySpanChar_IsGraphQLEscapedUnicodeCharacter(string text, bool result)
        {
            Assert.AreEqual(result, text.AsSpan().IsGraphQLEscapedUnicodeCharacter());
        }

        [TestCase(@"\r", true)]
        [TestCase(@"\n", true)]
        [TestCase(@"\/", true)]
        [TestCase(@"\\", true)]
        [TestCase(@"\b", true)]
        [TestCase(@"\f", true)]
        [TestCase(@"\t", true)]
        [TestCase("\\\"", true)]
        [TestCase(@"\a", false)]
        [TestCase(@"\3", false)]
        [TestCase(@"\", false)]
        [TestCase(@"", false)]
        [TestCase(@"abc", false)]
        public void ReadOnlySpanChar_IsGraphQLEscapedCharacter(string text, bool result)
        {
            Assert.AreEqual(result, text.AsSpan().IsGraphQLEscapedCharacter());
        }

        [TestCase(@"", false)]
        [TestCase(@"\", true)]
        [TestCase(@"\u", true)]
        [TestCase(@"\u12", true)]
        [TestCase(@"\ua", true)]
        [TestCase(@"\y", false)]
        [TestCase(@"\1", false)]
        [TestCase(@"\u12345", false)]
        public void ReadOnlySpanChar_CouldBeGraphQLEscapedUnicodeCharacter(string text, bool result)
        {
            Assert.AreEqual(result, text.AsSpan().CouldBeGraphQLEscapedUnicodeCharacter());
        }
    }
}