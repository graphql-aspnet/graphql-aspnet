// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Tests.Execution.ListManglerTestData;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using NUnit.Framework;

    [TestFixture]
    public class ListManglerTests
    {
        private static List<object[]> _listSource;

        private static void AddListItem(object data, Type targetType, bool shouldBeMangled)
        {
            _listSource.Add(new object[] { data, targetType, shouldBeMangled });
        }

        static ListManglerTests()
        {
            _listSource = new List<object[]>();

            // Source: List<int>
            var list = new List<int>() { 1, 2, 3, 4 };
            AddListItem(list, typeof(int[]), true);
            AddListItem(list, typeof(List<int>), false);
            AddListItem(list, typeof(IEnumerable<int>), false);
            AddListItem(list, typeof(ICollection<int>), false);
            AddListItem(list, typeof(IReadOnlyCollection<int>), false);
            AddListItem(list, typeof(IReadOnlyList<int>), false);

            // Source: int[]
            var list1 = new int[] { 1, 2, 3, 4 };
            AddListItem(list1, typeof(int[]), false);
            AddListItem(list1, typeof(List<int>), true);
            AddListItem(list1, typeof(IEnumerable<int>), false);
            AddListItem(list1, typeof(ICollection<int>), false);
            AddListItem(list1, typeof(IReadOnlyCollection<int>), false);
            AddListItem(list1, typeof(IReadOnlyList<int>), false);

            // Source: List<List<int>>
            var list2 = new List<List<int>>() { new List<int> { 1, 2 }, new List<int> { 3, 4 } };
            AddListItem(list2, typeof(int[][]), true);
            AddListItem(list2, typeof(List<int[]>), true);
            AddListItem(list2, typeof(List<int>[]), true);
            AddListItem(list2, typeof(IEnumerable<IEnumerable<int>>), false);
            AddListItem(list2, typeof(IEnumerable<int>[]), true);
            AddListItem(list2, typeof(IEnumerable<int[]>), true);
            AddListItem(list2, typeof(ICollection<ICollection<int>>), true);
            AddListItem(list2, typeof(ICollection<int>[]), true);
            AddListItem(list2, typeof(ICollection<int[]>), true);
            AddListItem(list2, typeof(ICollection<IEnumerable<int>>), true);
            AddListItem(list2, typeof(IEnumerable<ICollection<int>>), false);

            // Source: int[][]
            var list3 = new int[][] { new int[] { 1, 2 }, new int[] { 3, 4 } };
            AddListItem(list3, typeof(int[][]), false);
            AddListItem(list3, typeof(List<int[]>), true);
            AddListItem(list3, typeof(List<int>[]), true);
            AddListItem(list3, typeof(IEnumerable<IEnumerable<int>>), false);
            AddListItem(list3, typeof(IEnumerable<int>[]), false);
            AddListItem(list3, typeof(IEnumerable<int[]>), false);
            AddListItem(list3, typeof(ICollection<ICollection<int>>), false);
            AddListItem(list3, typeof(ICollection<int>[]), false);
            AddListItem(list3, typeof(ICollection<int[]>), false);
            AddListItem(list3, typeof(ICollection<IEnumerable<int>>), false);
            AddListItem(list3, typeof(IEnumerable<ICollection<int>>), false);

            var list4 = new List<int[]> { new int[] { 1, 2, }, new int[] { 3, 4 } };
            AddListItem(list4, typeof(int[][]), true);
            AddListItem(list4, typeof(List<int[]>), false);
            AddListItem(list4, typeof(List<int>[]), true);
            AddListItem(list4, typeof(IEnumerable<IEnumerable<int>>), false);
            AddListItem(list4, typeof(IEnumerable<int>[]), true);
            AddListItem(list4, typeof(IEnumerable<int[]>), false);
            AddListItem(list4, typeof(ICollection<ICollection<int>>), true);
            AddListItem(list4, typeof(ICollection<int>[]), true);
            AddListItem(list4, typeof(ICollection<int[]>), false);
            AddListItem(list4, typeof(ICollection<IEnumerable<int>>), true);
            AddListItem(list4, typeof(IEnumerable<ICollection<int>>), false);

            var list5 = new List<int>[] { new List<int>() { 1, 2, }, new List<int>() { 3, 4 } };
            AddListItem(list5, typeof(int[][]), true);
            AddListItem(list5, typeof(List<int[]>), true);
            AddListItem(list5, typeof(List<int>[]), false);
            AddListItem(list5, typeof(IEnumerable<IEnumerable<int>>), false);
            AddListItem(list5, typeof(IEnumerable<int>[]), false);
            AddListItem(list5, typeof(IEnumerable<int[]>), true);
            AddListItem(list5, typeof(ICollection<ICollection<int>>), false);
            AddListItem(list5, typeof(ICollection<int>[]), false);
            AddListItem(list5, typeof(ICollection<int[]>), true);
            AddListItem(list5, typeof(ICollection<IEnumerable<int>>), false);
            AddListItem(list5, typeof(IEnumerable<ICollection<int>>), false);

            // Source:  List<List<List<List<List<int>>>>>
            var list6 = new List<List<List<List<List<int>>>>>()
            {
                new List<List<List<List<int>>>>()
                {
                    new List<List<List<int>>>()
                    {
                        new List<List<int>>()
                        {
                            new List<int>()
                            {
                                1, 2,
                            },
                        },
                        new List<List<int>>()
                        {
                            new List<int>()
                            {
                                3, 4,
                            },
                        },
                    },
                    new List<List<List<int>>>()
                    {
                        new List<List<int>>()
                        {
                            new List<int>()
                            {
                                5, 6,
                            },
                        },
                        new List<List<int>>()
                        {
                            new List<int>()
                            {
                                7, 8,
                            },
                        },
                    },
                },

                new List<List<List<List<int>>>>()
                {
                    new List<List<List<int>>>()
                    {
                        new List<List<int>>()
                        {
                            new List<int>()
                            {
                                9, 10,
                            },
                        },
                        new List<List<int>>()
                        {
                            new List<int>()
                            {
                                11, 12,
                            },
                        },
                    },
                    new List<List<List<int>>>()
                    {
                        new List<List<int>>()
                        {
                            new List<int>()
                            {
                                13, 14,
                            },
                        },
                        new List<List<int>>()
                        {
                            new List<int>()
                            {
                                15, 16,
                            },
                        },
                    },
                },
            };

            AddListItem(list6, typeof(int[][][][][]), true);
            AddListItem(list6, typeof(List<int[][][]>[]), true);
            AddListItem(list6, typeof(List<List<int[]>[]>[]), true);
            AddListItem(list6, typeof(List<List<List<List<List<int>>>>>), false);
            AddListItem(list6, typeof(IEnumerable<IEnumerable<IEnumerable<IEnumerable<IEnumerable<int>>>>>), false);
            AddListItem(list6, typeof(List<IEnumerable<ICollection<IReadOnlyCollection<IList<int>>>>>), true);

            AddListItem(null, typeof(int[]), false);
            AddListItem(null, typeof(List<int>), false);

            var objList = new List<MangledTeacher>();
            objList.Add(new MangledTeacher()
            {
                Subject = "math",
                Name = "Ms. Smith",
            });
            objList.Add(new MangledTeacher()
            {
                Subject = "history",
                Name = "Mr. Jones",
            });

            // Teacher inherits from person
            AddListItem(objList, typeof(List<MangledTeacher>), false);
            AddListItem(objList, typeof(List<MangledPerson>), true);
            AddListItem(objList, typeof(IEnumerable<MangledPerson>), false);
            AddListItem(objList, typeof(MangledPerson[]), true);
            AddListItem(objList, typeof(MangledTeacher[]), true);
        }

        [TestCaseSource(nameof(_listSource))]
        public void WhenTypeProcessingShouldBeSuccessful_ItIsSuccessful(
            object inputData,
            Type expectedOutputType,
            bool shouldBeMangled)
        {
            var listCreator = new ListMangler(expectedOutputType);
            var result = listCreator.Convert(inputData);

            Assert.IsNotNull(result);

            if (inputData == null)
            {
                Assert.IsNull(result.Data);
                Assert.IsFalse(result.IsChanged);
                return;
            }

            var enumerable = result.Data as IEnumerable;
            Assert.IsNotNull(enumerable);
            Assert.IsTrue(Validation.IsCastable(enumerable.GetType(), expectedOutputType));

            Assert.AreEqual(
                shouldBeMangled,
                result.IsChanged,
                $"Input type {inputData.GetType().FriendlyName()} should " +
                $"{(shouldBeMangled ? string.Empty : "not")} have been mangled when converting " +
                $"to {expectedOutputType.FriendlyName()} but {(shouldBeMangled ? "was not" : "was")}");

            CollectionAssert.AreEqual(inputData as IEnumerable, enumerable);
        }

        [Test]
        public void WhenIncompatibleListTypes_ExceptionIsThrown()
        {
            var list = new int[] { 1, 2, 3, 4 };
            var listCreator = new ListMangler(typeof(List<List<int>>));

            Assert.Throws<InvalidOperationException>(() =>
            {
                // int[] (1 level deep) and List<List<int>> (2 levels deep)
                // are incompatiable
                var result = listCreator.Convert(list);
            });
        }

        [Test]
        public void WhenIncompatibleCoreTypes_ExceptionIsThrown()
        {
            var list = new TwoPropertyObject[]
            {
                new TwoPropertyObject()
                {
                    Property1 = "str",
                    Property2 = 5,
                },

                new TwoPropertyObject()
                {
                    Property1 = "str1",
                    Property2 = 6,
                },
            };

            var listCreator = new ListMangler(typeof(List<TwoPropertyObjectV2>));

            Assert.Throws<InvalidOperationException>(() =>
            {
                // cannot cast TwoPropertyObject to TwoPropertyObjectV2
                var result = listCreator.Convert(list);
            });
        }
    }
}