// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Extensions
{
    using System;
    using GraphQL.AspNet.Common.Extensions;
    using NUnit.Framework;

    [TestFixture]
    public class StringExtensionTests
    {
        [TestCase("abc12345", '7', new[] { '3' }, "abc12745")]
        [TestCase("", '7', new[] { '3' }, "")]
        [TestCase(null, 'N', new[] { '7' }, null)]
        [TestCase("abc12345", '7', null, "abc12345")]
        [TestCase("abc12", '7', new[] { '3' }, "abc12")]
        [TestCase("abc12345", 'X', new[] { '1', '2', '3', '4', '5' }, "abcXXXXX")]
        public void ReplaceAll(string text, char replaceWith, char[] charsToReplace, string output)
        {
            Assert.AreEqual(output, text.ReplaceAll(replaceWith, charsToReplace));
        }

        [TestCase("abc12345", 3, "345")]
        [TestCase("abc12345", 8, "abc12345")]
        [TestCase("abc12345", 0, "")]
        [TestCase("", 0, "")]
        public void SubStringRight(string text, int length, string expectedOutput)
        {
            Assert.AreEqual(expectedOutput, text.SubstringRight(length));
        }

        [TestCase("", 1)]
        [TestCase("abc123", -1)]
        [TestCase("abc123", 55)]
        public void SubStringRight_InvalidLength_ThrowsException(string text, int length)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var str = text.SubstringRight(length);
            });
        }

        [TestCase("", -1)]
        [TestCase("abc123", 0)]
        [TestCase("123abc123", 3)]
        [TestCase("123456", -1)]
        public void IndexOfFirstLetter(string text, int expectedIndex)
        {
            var index = text.IndexOfFirstAlphaChar();
            Assert.AreEqual(expectedIndex, index);
        }

        [TestCase("", -1)]
        [TestCase("abc123", 5)]
        [TestCase("123abc", 2)]
        [TestCase("1abc", 0)]
        [TestCase("123456", 5)]
        public void IndexOfLastDigit(string text, int expectedIndex)
        {
            var index = text.IndexOfLastDigit();
            Assert.AreEqual(expectedIndex, index);
        }

        [TestCase("123", "abc", "456", "123")]
        [TestCase("ABC", "abc", "123", "123")]
        [TestCase("ABC", "aBc", "123", "123")]
        [TestCase("", "aBc", "123", "")]
        public void ReplaceCaseInvariant(string text, string oldText, string newText, string expected)
        {
            var output = text.ReplaceCaseInvariant(oldText, newText);
            Assert.AreEqual(expected, output);
        }

        [TestCase("", "", true)]
        [TestCase("ABC", "abc", true)]
        [TestCase("ABC", "aBc", true)]
        [TestCase("", "aBc", false)]
        public void EqualsCaseInvariant(string text, string compareTo, bool expectedEqualness)
        {
            var output = text.EqualsCaseInvariant(compareTo);
            Assert.AreEqual(expectedEqualness, output);
        }

        [TestCase("", "", true)]
        [TestCase("abc123", "", true)]
        [TestCase("ABC123", "abc", true)]
        [TestCase("ABC", "123", false)]
        [TestCase("", "aBc", false)]
        [TestCase("abc", null, false)]
        [TestCase(null, "abc", false)]
        public void ContainsCaseInvariant(string text, string compareTo, bool expectedEqualness)
        {
            var output = text.ContainsCaseInvariant(compareTo);
            Assert.AreEqual(expectedEqualness, output);
        }

        [TestCase("", "", true)]
        [TestCase("abc123", "abc", true)]
        [TestCase("ABC123", "abc", true)]
        [TestCase("ABC", "123", false)]
        [TestCase("", "aBc", false)]
        [TestCase("abc", null, false)]
        [TestCase(null, "abc", false)]
        public void StartsWithCaseInvariant(string text, string startsWith, bool expectedEqualness)
        {
            var output = text.StartsWithCaseInvariant(startsWith);
            Assert.AreEqual(expectedEqualness, output);
        }

        [TestCase("", "", true)]
        [TestCase("123abc", "abc", true)]
        [TestCase("123AbC", "abc", true)]
        [TestCase("123AbC", "abcabcabacabc", false)]
        [TestCase("ABC", "123", false)]
        [TestCase("", "aBc", false)]
        [TestCase("abc", null, false)]
        [TestCase(null, "abc", false)]
        public void EndsWithCaseInvariant(string text, string startsWith, bool expectedEqualness)
        {
            var output = text.EndsWithCaseInvariant(startsWith);
            Assert.AreEqual(expectedEqualness, output);
        }

        [TestCase("abcDeF123123", "def", "456", "abc456123123")]
        [TestCase("abc123123", "aBc", "456", "456123123")]
        [TestCase("abc123123", "123", "456", "abc123456")]
        [TestCase("abc123123", "123", "", "abc123")]
        [TestCase("abc", null, "123", "abc")]
        [TestCase("abc", "abc", null, "")]
        [TestCase("abc123", "abc", null, "123")]
        [TestCase(null, "abc", "123", null)]
        public void ReplaceLastInstanceOfCaseInvariant(string text, string oldValue, string newValue, string expected)
        {
            var output = text.ReplaceLastInstanceOfCaseInvariant(oldValue, newValue);
            Assert.AreEqual(expected, output);
        }

        [TestCase("abcDeF123123", "def", "456", "abc456123123")]
        [TestCase("abc123123", "aBc", "456", "456123123")]
        [TestCase("abc123123", "123", "456", "abc456123")]
        [TestCase("abc123123", "123", "", "abc123")]
        [TestCase("abc", null, "123", "abc")]
        [TestCase("abc", "abc", null, "")]
        [TestCase("abc123", "abc", null, "123")]
        [TestCase(null, "abc", "123", null)]
        public void ReplaceFirstInstanceOfCaseInvariant(string text, string oldValue, string newValue, string expected)
        {
            var output = text.ReplaceFirstInstanceOfCaseInvariant(oldValue, newValue);
            Assert.AreEqual(expected, output);
        }

        [TestCase("abc123", "abc123")]
        [TestCase("Abc123", "abc123")]
        [TestCase("\nAbc123", "\nAbc123")]
        [TestCase("456abc123", "456abc123")]
        public void FirstCharacterToLowerInvariant(string text, string expected)
        {
            var output = text.FirstCharacterToLowerInvariant();
            Assert.AreEqual(expected, output);
        }

        [TestCase("abc123", "abc123")]
        [TestCase("Abc123", "abc123")]
        [TestCase("_Abc123", "_Abc123")]
        [TestCase("\nAbc123", "\nAbc123")]
        [TestCase("456abc123", "456abc123")]
        public void ToJsonPropertyFormat(string text, string expected)
        {
            var output = text.ToJsonPropertyFormat();
            Assert.AreEqual(expected, output);
        }

        [TestCase("abc123", "Abc123")]
        [TestCase("Abc123", "Abc123")]
        [TestCase("\nAbc123", "\nAbc123")]
        [TestCase("456abc123", "456abc123")]
        public void FirstCharacterToUpperInvariant(string text, string expected)
        {
            var output = text.FirstCharacterToUpperInvariant();
            Assert.AreEqual(expected, output);
        }

        [TestCase("abcabcabc", "abc", 3)]
        [TestCase("abc123", "123", 1)]
        [TestCase("abc123", "abc", 1)]
        [TestCase("aaaaaaaaaa", "a", 10)]
        [TestCase("aaaaaaaaaa", "aa", 5)]
        [TestCase("aaaaaaaaaa", "", 0)]
        [TestCase("", "a", 0)]
        [TestCase("abc123", null, 0)]
        [TestCase(null, "abc123", 0)]
        public void FirstCharacterToUpperInvariant(string text, string phrase, int expectedCount)
        {
            var output = text.PhraseCount(phrase);
            Assert.AreEqual(expectedCount, output);
        }
    }
}