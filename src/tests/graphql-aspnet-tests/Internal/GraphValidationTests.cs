// *************************************************************
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
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using GraphQL.AspNet.Tests.Internal.GraphValidationTestData;
    using Moq;
    using NUnit.Framework;
    using GTW = GraphQL.AspNet.Schemas.TypeSystem.MetaGraphTypes;

    [TestFixture]
    public class GraphValidationTests
    {
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

        [TestCase(typeof(int), "int!", false, null)]
        [TestCase(typeof(int), "int", true, null)]
        [TestCase(typeof(IEnumerable<int>), "[int!]", false, null)]
        [TestCase(typeof(int[]), "[int!]", false, null)]
        [TestCase(typeof(IEnumerable<TwoPropertyObject>), "[TwoPropertyObject]", false, null)]
        [TestCase(typeof(TwoPropertyObject[]), "[TwoPropertyObject]", true, null)]
        [TestCase(typeof(TwoPropertyObject[]), "[TwoPropertyObject]", false, null)]
        [TestCase(typeof(int[][]), "[[int!]]", false, null)]
        [TestCase(typeof(string[]), "[string]", false, null)]
        [TestCase(typeof(KeyValuePair<string, int>), "KeyValuePair_string_int_!", false, null)]
        [TestCase(typeof(KeyValuePair<string[], int[][]>), "KeyValuePair_string___int_____!", false, null)]
        [TestCase(typeof(KeyValuePair<string, int[]>), "KeyValuePair_string_int___!", false, null)]
        [TestCase(typeof(KeyValuePair<string, int[]>[]), "[KeyValuePair_string_int___!]", false, null)]
        [TestCase(typeof(KeyValuePair<string[][], int[][][]>[]), "[KeyValuePair_string_____int_______!]", false, null)]
        [TestCase(typeof(KeyValuePair<string[][], int[][][]>[][]), "[[KeyValuePair_string_____int_______!]]", false, null)]
        [TestCase(typeof(KeyValuePair<string, int>[]), "[KeyValuePair_string_int_!]", false, null)]
        [TestCase(typeof(List<KeyValuePair<string, int>>), "[KeyValuePair_string_int_!]", false, null)]
        [TestCase(typeof(List<KeyValuePair<string, int>>), "[KeyValuePair_string_int_]", true, null)]
        [TestCase(typeof(IEnumerable<IEnumerable<int>>), "[[int!]]", false, null)]
        [TestCase(typeof(IEnumerable<IEnumerable<int>>), "int!", false, new GTW[] { GTW.IsNotNull })]
        public void GenerateTypeExpression(
            Type type,
            string expectedExpression,
            bool definesDefaultValue,
            GTW[] wrappers)
        {
            var mock = new Mock<IGraphTypeExpressionDeclaration>();
            mock.Setup(x => x.HasDefaultValue).Returns(definesDefaultValue);
            mock.Setup(x => x.TypeWrappers).Returns(wrappers);

            var typeExpression = GraphValidation.GenerateTypeExpression(type, mock.Object);
            Assert.AreEqual(expectedExpression, typeExpression.ToString());
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
