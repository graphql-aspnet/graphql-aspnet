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
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Tests.Common.CommonHelpers;
    using GraphQL.AspNet.Tests.Common.Interfaces;
    using GraphQL.AspNet.Tests.Framework.Interfaces;
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
            var list = new List<ISinglePropertyObject>();

            var obj1 = new TwoPropertyObject();
            var obj3 = new TwoPropertyObjectV3();
            list.Add(obj1);
            list.Add(obj3);

            var result = list.OfTypeButNotType<ISinglePropertyObject, TwoPropertyObject>();
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(obj3, result.First());
        }

        [Test]
        public void OfTypeButNotType_AgainstNull_ReturnsEmpty()
        {
            var result = LinqExtensions.OfTypeButNotType<ISinglePropertyObject, TwoPropertyObject>(null);
            CollectionAssert.IsEmpty(result);
        }
    }
}