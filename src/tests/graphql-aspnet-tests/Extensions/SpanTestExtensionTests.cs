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
    public class SpanTestExtensionTests
    {
        [TestCase("ABC_123", "_", '|', "ABC|123")]
        [TestCase("ABC\n123", "\n", '_', "ABC_123")]
        [TestCase("ABC_123", "_182", '|', "ABC|||3")]
        public void Replace_ReadOnlySpan_ReturnNewSpanReplaced(string text, string valuesToReplace, char newValue, string expectedText)
        {
            var spanStart = text.AsSpan();
            var spanNew = spanStart.Replace(valuesToReplace, newValue);
            Assert.AreEqual(expectedText, spanNew.ToString());
            Assert.AreEqual(text, spanStart.ToString());
        }

        [TestCase("ABC_123", "_", '|', "ABC|123")]
        [TestCase("ABC\n123", "\n", '_', "ABC_123")]
        [TestCase("ABC_123", "_182", '|', "ABC|||3")]
        public void Replace_Span_SpanIsUpdatedInPlace(string text, string valuesToReplace, char newValue, string expectedText)
        {
            var span = new Span<char>(text.ToCharArray());

            span.Replace(valuesToReplace, newValue);
            Assert.AreNotEqual(text, span.ToString());
            Assert.AreEqual(expectedText, span.ToString());
        }

        [TestCase("", "", true)]
        [TestCase("ABC", "abc", true)]
        [TestCase("ABC", "aBc", true)]
        [TestCase("", "aBc", false)]
        public void SpanChar_EqualsCaseInvariant(string text, string compareTo, bool expectedEqualness)
        {
            var span1 = text.AsSpan();
            var span2 = compareTo.AsSpan();

            Assert.AreEqual(expectedEqualness, span1.EqualsCaseInvariant(span2));
        }

        [Test]
        public void SpanChar_BothEmpty_EqualsCaseInvariant()
        {
            Assert.AreEqual(true, ReadOnlySpan<char>.Empty.EqualsCaseInvariant(ReadOnlySpan<char>.Empty));
        }

        [Test]
        public void SpanChar_SameReference_EqualsCaseInvariant()
        {
            var span = "abc123".AsSpan();
            Assert.AreEqual(true, span.EqualsCaseInvariant(span));
        }

        [Test]
        public void SpanChar_BothBlank_EqualsCaseInvariant()
        {
#pragma warning disable SA1122
            var span1 = "".AsSpan();
            var span2 = "".AsSpan();
#pragma warning restore SA1122
            Assert.AreEqual(true, span1.EqualsCaseInvariant(span2));
        }
    }
}