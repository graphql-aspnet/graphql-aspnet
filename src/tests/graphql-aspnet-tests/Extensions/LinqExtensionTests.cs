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
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Tests.CommonHelpers;
    using NUnit.Framework;

    [TestFixture]
    public class LinqExtensionTests
    {
        [TestCase("123")]
        [TestCase(123)]
        public void AsEnumerable_ReturnsItemInList(object item)
        {
            var asEnumerable = item.AsEnumerable();
            Assert.AreEqual(1, asEnumerable.Count());
            Assert.AreEqual(item, asEnumerable.ElementAt(0));
        }

        [Test]
        public void WhereIf_True_FiltersItems()
        {
            var lst = new[] { 1, 2, 3, 5, 6, 7, 8 };
            var filtered = lst.AsQueryable().WhereIf(true, x => x < 5);
            Assert.AreEqual(3, filtered.Count());
            Assert.IsTrue(filtered.Any(x => x == 1));
            Assert.IsTrue(filtered.Any(x => x == 2));
            Assert.IsTrue(filtered.Any(x => x == 3));
        }

        [Test]
        public void WhereIf_False_FiltersItems()
        {
            var lst = new[] { 1, 2, 3, 5, 6, 7, 8 };
            var orig = lst.AsQueryable();
            var filtered = orig.WhereIf(false, x => x < 5);
            Assert.AreEqual(filtered, orig);
            Assert.AreEqual(7, filtered.Count());
            Assert.IsTrue(filtered.Any(x => x == 1));
            Assert.IsTrue(filtered.Any(x => x == 2));
            Assert.IsTrue(filtered.Any(x => x == 3));
            Assert.IsTrue(filtered.Any(x => x == 5));
            Assert.IsTrue(filtered.Any(x => x == 6));
            Assert.IsTrue(filtered.Any(x => x == 7));
            Assert.IsTrue(filtered.Any(x => x == 8));
        }

        [TestCase(new[] { 1, 2, 3, 4, 5, 6, 7, 8 }, 15, 2, new int[] { })]
        [TestCase(new[] { 1, 2, 3, 4, 5, 6, 7, 8 }, 3, 0, new int[] { })]
        [TestCase(new[] { 1, 2, 3, 4, 5, 6, 7, 8 }, 3, 2, new[] { 5, 6 })]
        [TestCase(new[] { 1, 2, 3, 4, 5, 6, 7, 8 }, 1, 50, new[] { 1, 2, 3, 4, 5, 6, 7, 8 })]
        public void Page(int[] data, int page, int pageSize, int[] expectedOutput)
        {
            var pageReturned = data.AsQueryable().Page(page, pageSize);

            Assert.AreEqual(expectedOutput.Length, pageReturned.Count());
            for (var i = 0; i < expectedOutput.Length; i++)
            {
                Assert.AreEqual(expectedOutput[i], pageReturned.ElementAt(i));
            }
        }

        [Test]
        public void Page_InvalidPageNumber_ThrowsException()
        {
            var array = new int[] { 1, 2, 3 };
            Assert.Throws<ArgumentException>(() =>
            {
                var pageReturned = array.AsQueryable().Page(0, 1);
            });
        }

        [Test]
        public void Page_InvalidPageSize_ThrowsException()
        {
            var array = new int[] { 1, 2, 3 };
            Assert.Throws<ArgumentException>(() =>
            {
                var pageReturned = array.AsQueryable().Page(1, -5);
            });
        }

        [TestCase(new[] { 1, 2, 3, 4, 5, 6, 7, 8 }, 7, new int[] { 1 })]
        [TestCase(new[] { 1, 2, 3, 4, 5, 6, 7, 8 }, 8, new int[] { })]
        [TestCase(new[] { 1, 2, 3, 4, 5, 6, 7, 8 }, 45, new int[] { })]
        [TestCase(new[] { 1, 2, 3, 4, 5, 6, 7, 8 }, 2, new[] { 1, 2, 3, 4, 5, 6 })]
        public void SkipLastN(int[] data, int lastNToSkip, int[] expectedOutput)
        {
            var dataReturn = data.SkipLastN(lastNToSkip);
            Assert.AreEqual(expectedOutput.Length, dataReturn.Count());
            for (var i = 0; i < expectedOutput.Length; i++)
            {
                Assert.AreEqual(expectedOutput[i], dataReturn.ElementAt(i));
            }
        }

        [Test]
        public void SkipLastN_InvalidNCount_ThrowsException()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                var data = new[] { 1, 2, 3 };
                var dataReturn = data.SkipLastN(-1);

                var i = 0;
                foreach (var item in dataReturn)
                {
                    i += item;
                }
            });
        }

        [Test]
        public void OfTypeButNotType()
        {
            // V1 and V3 implement a  common interface
            var list = new List<ITwoPropertyObject>();

            var obj1 = new TwoPropertyObject();
            var obj3 = new TwoPropertyObjectV3();
            list.Add(obj1);
            list.Add(obj3);

            var result = list.OfTypeButNotType<ITwoPropertyObject, TwoPropertyObject>();
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(obj3, result.First());
        }

        [Test]
        public void OfTypeButNotType_AgainstNull_ReturnsEmpty()
        {
            var result = LinqExtensions.OfTypeButNotType<ITwoPropertyObject, TwoPropertyObject>(null);
            CollectionAssert.IsEmpty(result);
        }
    }
}