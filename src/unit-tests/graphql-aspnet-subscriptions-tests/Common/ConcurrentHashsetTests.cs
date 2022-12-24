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
    using System.Linq;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common.Generics;
    using NUnit.Framework;

    [TestFixture]
    public class ConcurrentHashsetTests
    {
        private class ThreeCharEqualityComparer : IEqualityComparer<string>
        {
            public bool Equals(string x, string y)
            {
                if (x == null)
                    return y == null;

                if (y == null)
                    return false;

                x = x.Length > 3 ? x.Substring(0, 3) : x;
                y = y.Length > 3 ? y.Substring(0, 3) : y;

                return x == y;
            }

            public int GetHashCode(string obj)
            {
                if (obj.Length < 3)
                    return obj.GetHashCode();

                return obj.Substring(0, 3).GetHashCode();
            }
        }

        [Test]
        public void Add_SingleItem_AddsCorrectly()
        {
            var hashSet = new ConcurrentHashSet<int>();
            hashSet.Add(5);

            Assert.IsTrue(hashSet.Contains(5));
            Assert.AreEqual(1, hashSet.Count);
        }

        [Test]
        public void Add_SameItemMultipleTimes_AddsCorrectly()
        {
            var hashSet = new ConcurrentHashSet<int>();
            for (var i = 0; i < 500; i++)
                hashSet.Add(5);

            Assert.IsTrue(hashSet.Contains(5));
            Assert.AreEqual(1, hashSet.Count);
        }

        [Test]
        public void Add_MultipleTimes_AddsCorrectly()
        {
            var hashSet = new ConcurrentHashSet<int>();
            hashSet.Add(5);
            hashSet.Add(8);

            Assert.IsTrue(hashSet.Contains(5));
            Assert.IsTrue(hashSet.Contains(8));
            Assert.AreEqual(2, hashSet.Count);

            hashSet.Clear();
            Assert.AreEqual(0, hashSet.Count);
        }

        [Test]
        public void Clear_RemovesAllItems()
        {
            var hashSet = new ConcurrentHashSet<int>();
            hashSet.Add(5);
            hashSet.Add(8);

            Assert.AreEqual(2, hashSet.Count);

            hashSet.Clear();
            Assert.AreEqual(0, hashSet.Count);
        }

        [Test]
        public void CreateFromEnumerable_AddsCorrectly()
        {
            var list = new List<int>();
            list.Add(5);
            list.Add(5);
            list.Add(8);

            var hashSet = new ConcurrentHashSet<int>(list);

            Assert.IsTrue(hashSet.Contains(5));
            Assert.IsTrue(hashSet.Contains(8));
            Assert.AreEqual(2, hashSet.Count);
        }

        [Test]
        public void CopyTo_MultipleTimes_AddsCorrectly()
        {
            var hashSet = new ConcurrentHashSet<int>();
            hashSet.Add(5);
            hashSet.Add(8);

            var array = new int[2];
            ((ICollection<int>)hashSet).CopyTo(array, 0);

            Assert.IsTrue(array.Contains(5));
            Assert.IsTrue(array.Contains(8));
        }

        [Test]
        public void CopyTo_OutOfRange_ThrowsException()
        {
            var hashSet = new ConcurrentHashSet<int>();
            hashSet.Add(5);
            hashSet.Add(8);

            Assert.Throws<ArgumentException>(() =>
            {
                var array = new int[2];
                ((ICollection<int>)hashSet).CopyTo(array, 4);
            });
        }

        [Test]
        public void Remove_RemovesTheItem()
        {
            var hashSet = new ConcurrentHashSet<int>();
            hashSet.Add(5);
            Assert.IsTrue(hashSet.Contains(5));

            hashSet.TryRemove(5);
            Assert.AreEqual(0, hashSet.Count);
            Assert.IsFalse(hashSet.Contains(5));
        }

        [Test]
        public void EnumeratorCheck()
        {
            var hashSet = new ConcurrentHashSet<int>();
            hashSet.Add(3);
            hashSet.Add(4);
            hashSet.Add(5);
            hashSet.Add(6);
            hashSet.Add(7);

            var i = 0;
            foreach (var item in hashSet)
                i += item;

            Assert.AreEqual(25, i);
        }

        [Test]
        public void IsEmpty()
        {
            var hashSet = new ConcurrentHashSet<int>();

            Assert.IsTrue(hashSet.IsEmpty);
            hashSet.Add(3);

            Assert.IsFalse(hashSet.IsEmpty);
        }

        [Test]
        public void ConstructorOverLoad_CustomCapacityAndConcurrency_DoesNotFail()
        {
            var hashSet = new ConcurrentHashSet<int>(5, 5);
            hashSet.Add(3);
            hashSet.Add(4);
            hashSet.Add(5);
            hashSet.Add(6);
            hashSet.Add(7);
            Assert.AreEqual(5, hashSet.Count);
        }

        [Test]
        public void ConstructorOverLoad_CustomCapacityAndConcurrency_WithCapacityLessThanConcurrency_DoesNotFail()
        {
            var hashSet = new ConcurrentHashSet<int>(8, 1);
            hashSet.Add(3);
            hashSet.Add(4);
            hashSet.Add(5);
            Assert.AreEqual(3, hashSet.Count);
        }

        [Test]
        public void ConstructorOverLoad_CustomCapacityAndConcurrency_InvalidConcurrency_ThrowsException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ConcurrentHashSet<int>(-1, 5));
        }

        [Test]
        public void ConstructorOverLoad_CustomCapacityAndConcurrency_InvalidCapacity_ThrowsException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ConcurrentHashSet<int>(5, -1));
        }

        [Test]
        public void ConstructorOverLoad_CustomComparer()
        {
            var threeCharSet = new ConcurrentHashSet<string>(new ThreeCharEqualityComparer());
            threeCharSet.Add("string1");
            threeCharSet.Add("string2");
            Assert.AreEqual(1, threeCharSet.Count);
        }

        [Test]
        public void ConstructorOverLoad_CustomComparer_WithInitialCapacity()
        {
            var threeCharSet = new ConcurrentHashSet<string>(5, 5, new ThreeCharEqualityComparer());
            threeCharSet.Add("string1");
            threeCharSet.Add("string2");
            Assert.AreEqual(1, threeCharSet.Count);
        }

        [Test]
        public void ConstructorOverLoad_InitialCollection_CustomComparer()
        {
            var list = new List<string>();
            list.Add("string1");
            list.Add("string2");

            var threeCharSet = new ConcurrentHashSet<string>(5, list, new ThreeCharEqualityComparer());
            Assert.AreEqual(1, threeCharSet.Count);
        }

        [Test]
        public void ConstructorOverLoad_InitialCollection_ThatIsNull_DoesNotFail()
        {
            var threeCharSet = new ConcurrentHashSet<string>(5, null, new ThreeCharEqualityComparer());
            Assert.AreEqual(0, threeCharSet.Count);
        }

        [Test]
        public void CapacityCheck()
        {
            var hashSet = new ConcurrentHashSet<int>(4, 4);

            // force the hashset to grow in size at least once
            for (var i = 0; i < 32; i++)
                hashSet.Add(i);

            Assert.AreEqual(32, hashSet.Count);
        }

        [Test]
        public async Task Add_ViaMultipleTaskYields_AddsCorrectly()
        {
            async Task AddToHashSet(ConcurrentHashSet<int> setToAddTo)
            {
                await Task.Yield();
                for (var i = 0; i < 10000; i++)
                {
                    setToAddTo.Add(i);
                }
            }

            async Task RemoveFromHashSet(ConcurrentHashSet<int> setToAddTo)
            {
                await Task.Yield();
                for (var i = 0; i < 10000; i++)
                {
                    setToAddTo.TryRemove(i);
                }
            }

            var hashSet = new ConcurrentHashSet<int>();
            var tasks = new List<Task>();
            for (var i = 0; i < 100; i++)
            {
                var task = AddToHashSet(hashSet);
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);

            Assert.AreEqual(10000, hashSet.Count);
            for (var i = 0; i < 10000; i++)
            {
                 Assert.IsTrue(hashSet.Contains(i));
            }

            tasks.Clear();
            for (var i = 0; i < 100; i++)
            {
                var task = RemoveFromHashSet(hashSet);
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
            Assert.AreEqual(0, hashSet.Count);
        }
    }
}