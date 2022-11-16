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
    using System.Threading.Tasks;
    using NUnit.Framework;
    using GraphQL.AspNet.Common.Extensions;

    [TestFixture]
    public class TaskExtensionTests
    {
        private static object[] _resultOfTypeOrNullCases =
        {
            new object[] { 55, typeof(int), true },
            new object[] { 23.0, typeof(double), true },
            new object[] { new object(), typeof(object), true },
            new object[] { new List<int>(), typeof(IEnumerable<int>), true },

            new object[] { 55, typeof(double), false },
            new object[] { null, typeof(double), false },
            new object[] { new List<int>(), typeof(Dictionary<int, string>), false },
            new object[] { 55, typeof(double), false },
            new object[] { null, typeof(List<int>), false },
        };

        [TestCaseSource(nameof(_resultOfTypeOrNullCases))]
        public void Task_ResultOfTypeOrNull(object data, Type expectedType, bool shouldCast)
        {
            var task = Task.FromResult(data);
            var result = task.ResultOfTypeOrNull(expectedType);
            if (shouldCast)
                Assert.AreEqual(data, result);
            else
                Assert.IsNull(result);
        }

        [Test]
        public void Task_ResultOfTypeOrNull_NulLExpectedType_ThrowsException()
        {
            var task = Task.FromResult(55);
            Assert.Throws<ArgumentNullException>(() =>
            {
                var result = task.ResultOfTypeOrNull(null);
            });
        }

        [Test]
        public void Task_ResultOfTypeOrNull_NullTask_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                TaskTExtensions.ResultOfTypeOrNull(null, typeof(int));
            });
        }

        [Test]
        public void Task_ResultOfTypeOrNull_FaultedTaskReturnsNull()
        {
            var task = Task.FromException(new Exception("fake Exception"));
            var result = task.ResultOfTypeOrNull<int>();
            Assert.IsNull(result);
        }

        [Test]
        public void Task_ResultOfTypeOrNull_ValidValueTypeParamTest()
        {
            var task = Task.FromResult(55);
            var result = task.ResultOfTypeOrNull<int>();
            Assert.AreEqual(55, result);
        }

        [Test]
        public void Task_ResultOfTypeOrNull_InvalidValueTypeParamTest()
        {
            var task = Task.FromResult(55);
            var result = task.ResultOfTypeOrNull<double>();
            Assert.IsNull(result);
        }

        [Test]
        public void Task_ResultOfTypeOrNull_ValidReferenceTypeParamTest()
        {
            var list = new List<int>();
            list.Add(2);
            list.Add(-15);

            var task = Task.FromResult(list);
            var result = task.ResultOfTypeOrNull<IEnumerable<int>>();
            Assert.IsNotNull(result);
            CollectionAssert.AreEquivalent(list, result as IEnumerable<int>);
        }

        [Test]
        public void Task_ResultOfTypeOrNull_InvalidReferenceTypeParamTest()
        {
            var list = new List<int>();
            var task = Task.FromResult(list);
            var result = task.ResultOfTypeOrNull<Dictionary<int, string>>();
            Assert.IsNull(result);
        }
    }
}