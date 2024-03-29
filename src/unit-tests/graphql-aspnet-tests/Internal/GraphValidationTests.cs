﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Internal;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using GraphQL.AspNet.Tests.Internal.GraphValidationTestData;
    using NUnit.Framework;

    [TestFixture]
    public class GraphValidationTests
    {
        public delegate int MyTestDelegate(int arg1);

        public enum ValidationTestEnum
        {
            Value1,
            Value2,
        }

        [TestCase(typeof(IEnumerable<int>), true)]
        [TestCase(typeof(ICollection<int>), true)]
        [TestCase(typeof(IList<int>), true)]
        [TestCase(typeof(List<int>), true)]
        [TestCase(typeof(List<string>), true)]
        [TestCase(typeof(List<DateTime>), true)]
        [TestCase(typeof(List<TwoPropertyObject>), true)]
        [TestCase(typeof(string), false)]
        [TestCase(typeof(int), false)]
        [TestCase(typeof(DateTime), false)]
        [TestCase(typeof(Dictionary<string, int>), false)]
        [TestCase(typeof(IDictionary<string, int>), false)]
        [TestCase(typeof(IReadOnlyDictionary<string, int>), false)]
        [TestCase(typeof(TwoPropertyObject[]), true)]
        [TestCase(typeof(int[]), true)]
        [TestCase(typeof(string[]), true)]
        [TestCase(typeof(DateTime[]), true)]
        [TestCase(typeof(ValidationTestEnum[]), true)]
        [TestCase(typeof(KeyValuePair<string, string>[]), true)]
        [TestCase(typeof(KeyValuePair<string, TwoPropertyObject>[]), true)]
        [TestCase(typeof(KeyValuePair<string, int>[]), true)]
        [TestCase(typeof(IDictionary<string, string>), false)]
        [TestCase(typeof(IDictionary<string, TwoPropertyObject>), false)]
        [TestCase(typeof(IDictionary<string, int>), false)]
        [TestCase(typeof(IReadOnlyDictionary<string, string>), false)]
        [TestCase(typeof(IReadOnlyDictionary<string, TwoPropertyObject>), false)]
        [TestCase(typeof(IReadOnlyDictionary<string, int>), false)]
        [TestCase(typeof(Dictionary<string, string>), false)]
        [TestCase(typeof(Dictionary<string, TwoPropertyObject>), false)]
        [TestCase(typeof(Dictionary<string, int>), false)]
        public void IsValidListType(Type inputType, bool isValidListType)
        {
            var result = GraphValidation.IsValidListType(inputType);
            Assert.AreEqual(isValidListType, result);
        }

        [TestCase(typeof(IEnumerable<int>), true)]
        [TestCase(typeof(ICollection<int>), true)]
        [TestCase(typeof(IList<int>), true)]
        [TestCase(typeof(List<int>), true)]
        [TestCase(typeof(List<string>), true)]
        [TestCase(typeof(List<DateTime>), true)]
        [TestCase(typeof(List<TwoPropertyObject>), true)]
        [TestCase(typeof(TwoPropertyObject[]), true)]
        [TestCase(typeof(int[]), true)]
        [TestCase(typeof(string[]), true)]
        [TestCase(typeof(DateTime[]), true)]
        [TestCase(typeof(string), true)]
        [TestCase(typeof(int), true)]
        [TestCase(typeof(DateTime), true)]
        [TestCase(typeof(KeyValuePair<string, int>), true)]
        [TestCase(typeof(KeyValuePair<string, int>[]), true)]
        [TestCase(typeof(List<KeyValuePair<string, int>>), true)]
        [TestCase(typeof(Dictionary<string, int>), false)]
        [TestCase(typeof(IDictionary<string, int>), false)]
        [TestCase(typeof(IReadOnlyDictionary<string, int>), false)]
        [TestCase(typeof(object), false)]
        [TestCase(typeof(Func<int, bool>), false)]
        [TestCase(typeof(Func<int>), false)]
        [TestCase(typeof(Action<int, bool>), false)]
        [TestCase(typeof(Action), false)]
        [TestCase(typeof(MyTestDelegate), false)]
        public void IsValidGraphType(Type inputType, bool isValidType)
        {
            var result = GraphValidation.IsValidGraphType(inputType);
            Assert.AreEqual(isValidType, result);
            if (!result)
            {
                Assert.Throws<GraphTypeDeclarationException>(() =>
                {
                    GraphValidation.IsValidGraphType(inputType, true);
                });
            }
        }

        [TestCase(typeof(int), typeof(int), true, true, true)]
        [TestCase(typeof(Task<int>), typeof(int), true, true, true)]
        [TestCase(typeof(Task<int>), typeof(Task<int>), true, false, true)]
        [TestCase(typeof(int?), typeof(int), true, true, true)]
        [TestCase(typeof(Task<int?>), typeof(int), true, true, true)]
        [TestCase(typeof(Task<List<int?>>), typeof(int), true, true, true)]
        [TestCase(typeof(List<int>), typeof(int), true, true, true)]
        [TestCase(typeof(int?), typeof(int?), true, true, false)]
        [TestCase(typeof(int[]), typeof(int), true, true, true)]
        [TestCase(typeof(int[]), typeof(int[]), false, true, true)]
        [TestCase(typeof(string[]), typeof(string), true, true, true)]
        [TestCase(typeof(Task<int[]>), typeof(int), true, true, true)]
        [TestCase(typeof(Task<int?[]>), typeof(int), true, true, true)]
        [TestCase(typeof(Task<int?[]>), typeof(int?), true, true, false)]
        [TestCase(typeof(Task<int?[]>), typeof(Task<int?[]>), true, false, true)]
        public void EliminateWrappersFromCoreType(
            Type typeToTest,
            Type expectedCoreType,
            bool removeEnumerables,
            bool removeTask,
            bool removeNullableT)
        {
            var result = GraphValidation.EliminateWrappersFromCoreType(typeToTest, removeEnumerables, removeTask, removeNullableT);
            Assert.AreEqual(expectedCoreType, result);
        }

        [Test]
        public void RetrieveSecurityPolicies_NullAttributeProvider_YieldsNoPolicies()
        {
            var policies = GraphValidation.RetrieveSecurityPolicies(null);
            Assert.AreEqual(0, policies.Count());
        }

        [Test]
        public void RetrieveSecurityPolicies_MethodWithNoSecurityPolicies_ReturnsEmptyEnumerable()
        {
            var info = typeof(ControllerWithNoSecurityPolicies).GetMethod(nameof(ControllerWithNoSecurityPolicies.SomeMethod));

            var policies = GraphValidation.RetrieveSecurityPolicies(info);
            Assert.AreEqual(0, policies.Count());
        }

        [Test]
        public void RetrieveSecurityPolicies_MethodWithSecurityPolicies_ReturnsOneItem()
        {
            var info = typeof(ControllerWithSecurityPolicies).GetMethod(nameof(ControllerWithSecurityPolicies.SomeMethod));

            var policies = GraphValidation.RetrieveSecurityPolicies(info);
            Assert.AreEqual(1, policies.Count());
        }
    }
}