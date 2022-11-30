﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Common
{
    using GraphQL.AspNet.Common;
    using NUnit.Framework;

    [TestFixture]
    public class GraphQLStringsTests
    {
        [TestCase("\"abc\"", "abc")]
        [TestCase("\"\"\"abc\"\"\"", "abc")]
        [TestCase("\"\"\"abc\"\"", "\"\"abc\"")]
        [TestCase("\"abc\\u0245123\"", "abcɅ123")]
        [TestCase("\"a\\nbc\"", "a\nbc")]
        [TestCase("\"a\\n\\r\\0\\bbc\"", "a\n\r\0\bbc")]
        [TestCase("\"\\ucd5c\\uc608\\ub098\"", "최예나")]
        public void UnescapeAndTrimDelimiters(string inputText, string expectedOutput)
        {
            var result = GraphQLStrings.UnescapeAndTrimDelimiters(inputText);

            Assert.AreEqual(
                expectedOutput,
                result,
                $"Expected '{expectedOutput}' but got '{result}'");
        }
    }
}