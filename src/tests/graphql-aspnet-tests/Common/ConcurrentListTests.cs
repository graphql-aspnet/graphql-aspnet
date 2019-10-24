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
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common.Generics;
    using NUnit.Framework;

    [TestFixture]
    public class ConcurrentListTests
    {
        [Test]
        public void Add_SingleItem_AddsCorrectly()
        {
            var list = new ConcurrentList<int>();
            list.Add(5);

            Assert.IsTrue(list.Contains(5));
            Assert.AreEqual(1, list.Count);
            Assert.IsFalse(list.IsReadOnly);
        }

        [Test]
        public void Enumerate()
        {
            var list = new ConcurrentList<int>();
            list.Add(5);
            list.Add(6);
            list.Add(7);

            var enumerable = list as IEnumerable;

            var i = 0;
            foreach (var _ in enumerable)
                i++;

            Assert.AreEqual(3, i);
        }

        [Test]
        public void Add_MultipleTimes_AddsCorrectly()
        {
            var list = new ConcurrentList<int>();
            list.Add(5);
            list.Add(8);

            Assert.IsTrue(list.Contains(5));
            Assert.IsTrue(list.Contains(8));
            Assert.AreEqual(2, list.Count);

            list.Clear();
            Assert.AreEqual(0, list.Count);
        }

        [Test]
        public void Clear_RemovesAllItems()
        {
            var list = new ConcurrentList<int>();
            list.Add(5);
            list.Add(8);

            Assert.AreEqual(2, list.Count);

            list.Clear();
            Assert.AreEqual(0, list.Count);
        }

        [Test]
        public void CreateFromEnumerable_AddsCorrectly()
        {
            var list = new List<int>();
            list.Add(5);
            list.Add(5);
            list.Add(8);

            var concurrentlist = new ConcurrentList<int>(list);

            Assert.IsTrue(concurrentlist.Contains(5));
            Assert.IsTrue(concurrentlist.Contains(8));
            Assert.AreEqual(list.Count, concurrentlist.Count);
        }

        [Test]
        public void Insert_AddsCorrectly()
        {
            var list = new ConcurrentList<int>();
            list.Add(5);
            list.Add(6);
            list.Add(8);

            list.Insert(1, 15);
            Assert.AreEqual(4, list.Count);
            Assert.AreEqual(15, list[1]);
            list[3] = 22;

            Assert.AreEqual(22, list[3]);
        }

        [Test]
        public void IndexOf_ReturnsCorrectly()
        {
            var list = new ConcurrentList<int>();
            list.Add(5);
            list.Add(6);
            list.Add(8);

            Assert.AreEqual(1, list.IndexOf(6));
            Assert.AreEqual(-1, list.IndexOf(55));
        }

        [Test]
        public void RemoveAt_IsRemoved()
        {
            var list = new ConcurrentList<int>();
            list.Add(5);
            list.Add(6);
            list.Add(7);

            Assert.AreEqual(3, list.Count);
            list.RemoveAt(1);

            Assert.AreEqual(2, list.Count);
            Assert.IsFalse(list.Contains(6));
        }

        [Test]
        public void CopyTo_MultipleTimes_AddsCorrectly()
        {
            var list = new ConcurrentList<int>();
            list.Add(5);
            list.Add(8);

            var array = new int[2];
            ((ICollection<int>)list).CopyTo(array, 0);

            Assert.IsTrue(array.Contains(5));
            Assert.IsTrue(array.Contains(8));
        }

        [Test]
        public void CopyTo_OutOfRange_ThrowsException()
        {
            var list = new ConcurrentList<int>();
            list.Add(5);
            list.Add(8);

            Assert.Throws<ArgumentException>(() =>
            {
                var array = new int[2];
                ((ICollection<int>)list).CopyTo(array, 4);
            });
        }

        [Test]
        public void Remove_RemovesTheItem()
        {
            var list = new ConcurrentList<int>();
            list.Add(5);
            Assert.IsTrue(list.Contains(5));

            list.Remove(5);
            Assert.AreEqual(0, list.Count);
            Assert.IsFalse(list.Contains(5));
        }

        [Test]
        public async Task Add_MultiThreads_AddsCorrectly()
        {
            async Task AddTolist(ConcurrentList<int> setToAddTo, int start, int stop)
            {
                await Task.Yield();
                for (var i = start; i < stop; i++)
                {
                    setToAddTo.Add(i);
                }
            }

            async Task RemoveFromlist(ConcurrentList<int> setToAddTo, int start, int stop)
            {
                await Task.Yield();
                for (var i = start; i < stop; i++)
                {
                    setToAddTo.Remove(i);
                }
            }

            var list = new ConcurrentList<int>();
            var tasks = new List<Task>();
            for (var i = 0; i < 100; i++)
            {
                var task = AddTolist(list, i + (i * 100), (i * 100) + i + 100);
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);

            Assert.AreEqual(10000, list.Count);

            tasks.Clear();
            for (var i = 0; i < 100; i++)
            {
                var task = RemoveFromlist(list, i + (i * 100), (i * 100) + i + 100);
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
            Assert.AreEqual(0, list.Count);
        }
    }
}