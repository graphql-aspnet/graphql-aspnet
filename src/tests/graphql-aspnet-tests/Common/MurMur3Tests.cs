// *************************************************************
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
    public class MurMur3Tests
    {
        [Test]
        public void PerformingAHash_YieldsAStaticHashValue()
        {
            var text = @"Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's
                            standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book
                            It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged. It
                            was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages, and more recently with
                            desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum.";

            var initialHash = MurMurHash3.Hash(text);
            var secondHash = MurMurHash3.Hash(text);
            Assert.AreEqual(initialHash, secondHash);
        }

        [TestCase("abcĦ")]
        [TestCase("abcd")]
        [TestCase("abcde")]
        [TestCase("abcdef")]
        [TestCase(null)]
        public void PerformingAHash_SmallValueTests(string text)
        {
            // to exercise all cases of chunking the stream
            var initialHash = MurMurHash3.Hash(text);
            var secondHash = MurMurHash3.Hash(text);
            Assert.AreEqual(initialHash, secondHash);
        }

        [TestCase("abcĦ", "abc")]
        [TestCase("abcd", "abc")]
        [TestCase("abcde", "abcd")]
        [TestCase("abcdef", "abcde")]
        [TestCase("abc", null)]
        public void PerformingAHash_AreDifferentValues(string text1, string text2)
        {
            // to exercise all cases of chunking the stream
            var firstHash = MurMurHash3.Hash(text1);
            var secondHash = MurMurHash3.Hash(text2);
            Assert.AreNotEqual(firstHash, secondHash);
        }
    }
}