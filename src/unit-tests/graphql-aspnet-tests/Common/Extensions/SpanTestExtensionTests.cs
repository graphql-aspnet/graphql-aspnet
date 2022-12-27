// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Common.Extensions
{
    using System;
    using GraphQL.AspNet.Common.Extensions;
    using NUnit.Framework;

    [TestFixture]
    public class SpanTestExtensionTests
    {
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