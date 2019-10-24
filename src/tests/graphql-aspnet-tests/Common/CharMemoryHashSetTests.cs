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
    using System;
    using System.Collections.Generic;
    using GraphQL.AspNet.Common.Generics;
    using NUnit.Framework;

    [TestFixture]
    public class CharMemoryHashSetTests
    {
        [Test]
        public void AddSingleItemTwice_IsSetOnce()
        {
            var memory1 = "abc123".AsMemory();

            var hashSet = new CharMemoryHashSet();
            hashSet.Add(memory1);
            hashSet.Add(memory1);

            Assert.AreEqual(1, hashSet.Count);
        }

        [Test]
        public void AddEquivilantButDifferentItems_IsSetOnce()
        {
            var memory1 = "abc123".AsMemory();
            var memory2 = "abc123".AsMemory();

            var hashSet = new CharMemoryHashSet();
            hashSet.Add(memory1);
            hashSet.Add(memory2);

            Assert.AreEqual(1, hashSet.Count);
        }

        [Test]
        public void AddRangeOfSameItem_IsSetOnce()
        {
            var memory1 = "abc123".AsMemory();
            var list = new List<ReadOnlyMemory<char>>();
            list.Add(memory1);
            list.Add(memory1);
            list.Add(memory1);
            list.Add(memory1);

            var hashSet = new CharMemoryHashSet();
            hashSet.AddRange(list);

            Assert.AreEqual(1, hashSet.Count);
        }

        [Test]
        public void AddRangeOfEquivilantItems_IsSetOnce()
        {
            var memory1 = "abc123".AsMemory();
            var memory2 = "abc123".AsMemory();
            var list = new List<ReadOnlyMemory<char>>();
            list.Add(memory1);
            list.Add(memory2);

            var hashSet = new CharMemoryHashSet();
            hashSet.AddRange(list);

            Assert.AreEqual(1, hashSet.Count);
        }

        [Test]
        public void AddRangeOfDifferentItems_AllAreSet()
        {
            var memory1 = "abc123".AsMemory();
            var memory2 = "abc1234".AsMemory();
            var list = new List<ReadOnlyMemory<char>>();
            list.Add(memory1);
            list.Add(memory2);

            var hashSet = new CharMemoryHashSet();
            hashSet.AddRange(list);

            Assert.AreEqual(2, hashSet.Count);
        }
    }
}