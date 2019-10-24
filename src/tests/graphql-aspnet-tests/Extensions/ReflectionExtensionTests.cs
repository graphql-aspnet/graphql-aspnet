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
    using System.Threading.Tasks;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Tests.Extensions.ReflectionExtensionTestData;
    using GraphQL.AspNet.Tests.ThirdPartyDll;
    using GraphQL.AspNet.Tests.ThirdPartyDll.Model;
    using NUnit.Framework;

    [TestFixture]
    public class ReflectionExtensionTests
    {
        [Test]
        public void LocateTypesInAssembly_OnAttributesOrClasses_ReturnsExpectedType()
        {
            var assembly = typeof(CustomerController).Assembly;
            var types = assembly.LocateTypesInAssembly(typeof(CandleSchema), typeof(GraphTypeAttribute));
            Assert.IsTrue(types.Contains(typeof(Candle)));
            Assert.IsTrue(types.Contains(typeof(CandleSchema)));
        }

        [Test]
        public void LocateTypesInAssembly_NoTypesProvided_ReturnsEmptyList()
        {
            var assembly = typeof(CustomerController).Assembly;
            var types = assembly.LocateTypesInAssembly();
            Assert.AreEqual(0, types.Count());
        }

        [Test]
        public void LocateTypesInAssembly_NoAssemblyProvided_ReturnsEmptyList()
        {
            var types = ReflectionExtensions.LocateTypesInAssembly(null);
            Assert.AreEqual(0, types.Count());
        }

        [TestCase(typeof(ObjectWithValidation), true, true)]
        [TestCase(typeof(ObjectWithoutValidation), true, false)]
        [TestCase(typeof(ObjectWithValidationOnChild), false, false)]
        [TestCase(typeof(ObjectWithValidationOnChild), true, true)]
        [TestCase(typeof(int), true, false)]
        [TestCase(typeof(string), true, false)]
        [TestCase(null, true, false)]
        public void RequiredValidation(Type typeToCheck, bool checkChildren, bool expectedResult)
        {
            var requiresValidation = typeToCheck.RequiresValidation(checkChildren);
            Assert.AreEqual(expectedResult, requiresValidation);
        }

        public void RequiresValidation_InvalidMaxDepth()
        {
            var reqValidation = typeof(ObjectWithValidation).RequiresValidation(true, -5);
            Assert.IsFalse(reqValidation);
        }

        [TestCase(typeof(int), "int")]
        [TestCase(typeof(int?), "int?")]
        [TestCase(typeof(long?), "long?")]
        [TestCase(typeof(IEnumerable<int>), "IEnumerable<int>")]
        [TestCase(typeof(IDictionary<,>), "IDictionary<TKey, TValue>")]
        [TestCase(typeof(IEnumerable<int>), "IEnumerable_int_", false, "_")]
        [TestCase(typeof(Task<IEnumerable<int>>), "Task<IEnumerable<int>>")]
        [TestCase(typeof(Task<IEnumerable<int>>), "Task_IEnumerable_int__", false, "_")]
        [TestCase(typeof(IDictionary<int, string>), "IDictionary<int, string>")]
        [TestCase(typeof(IDictionary<int, string>), "IDictionary_int_string_", false, "_")]
        [TestCase(typeof(Task<IDictionary<int, string>>), "Task<IDictionary<int, string>>")]
        [TestCase(typeof(Task<IDictionary<DateTimeExtensionTests, ReflectionExtensionTests>>), "Task<IDictionary<DateTimeExtensionTests, ReflectionExtensionTests>>")]
        public void Type_FriendlyName(Type type, string expectedName, bool includeCarrots = true, string delimiter = "")
        {
            // non generic type just returns Type.Name
            if (includeCarrots)
                Assert.AreEqual(expectedName, type.FriendlyName());
            else
                Assert.AreEqual(expectedName, type.FriendlyName(delimiter));
        }

        [TestCase(typeof(int), "System.Int32")]
        [TestCase(typeof(int?), "System.Nullable<System.Int32>")]
        [TestCase(typeof(IDictionary<int, string>), "System.Collections.Generic.IDictionary<System.Int32, System.String>")]
        public void Type_FriendlyName_WithNamespace(Type type, string expectedName)
        {
            var result = type.FriendlyName(true);
            Assert.AreEqual(expectedName, result);
        }

        [TestCase(typeof(int?), true)]
        [TestCase(typeof(int), false)]
        [TestCase(typeof(string), false)]
        public void Type_NullableOfT(Type type, bool isNullableOfT)
        {
            Assert.AreEqual(isNullableOfT, type.IsNullableOfT());
        }

        [TestCase(typeof(IEnumerable<int?>), true)]
        [TestCase(typeof(IEnumerable<int>), false)]
        [TestCase(typeof(IEnumerable<string>), false)]
        [TestCase(typeof(List<string>), false)]
        [TestCase(typeof(List<int?>), true)]
        [TestCase(typeof(Dictionary<string, int?>), false)]
        [TestCase(null, false)]
        public void Type_IsNullableEnumerableOfT(Type type, bool isNullableEnumerableOfT)
        {
            Assert.AreEqual(isNullableEnumerableOfT, type.IsNullableEnumerableOfT());
        }

        [Test]
        public void Type_SingleAttributeOrDefault_OneInstance_ReturnsInstance()
        {
            var attrib = typeof(SingleAttributeClass).SingleAttributeOrDefault<System.ComponentModel.DescriptionAttribute>();
            Assert.IsNotNull(attrib);
        }

        [Test]
        public void Type_SingleAttributeOrDefault_NoInstance_ReturnsNull()
        {
            var attrib = typeof(SingleAttributeClass).SingleAttributeOrDefault<GraphSkipAttribute>();
            Assert.IsNull(attrib);
        }

        [Test]
        public void Type_SingleAttributeOrDefault_MultipleInstance_ReturnsNull()
        {
            var attrib = typeof(MultiAttributeClass).SingleAttributeOrDefault<MultiAllowedAttribute>();
            Assert.IsNull(attrib);
        }

        [TestCase(typeof(SingleAttributeClass), typeof(System.ComponentModel.DescriptionAttribute), true)] // defined
        [TestCase(typeof(SingleAttributeClass), typeof(GraphSkipAttribute), false)] // not defined
        [TestCase(typeof(SingleAttributeClass), typeof(ReflectionExtensionTests), false)] // not an attribute
        [TestCase(typeof(SingleAttributeClass), null, false)]
        [TestCase(null, typeof(GraphSkipAttribute), false)]
        public void HasAttribute(Type typeToCheck, Type attributeType, bool expectedResult)
        {
            var result = typeToCheck.HasAttribute(attributeType);
            Assert.AreEqual(expectedResult, result);
        }

        [TestCase(typeof(int), false)]
        [TestCase(typeof(int?), true)]
        [TestCase(typeof(float), false)]
        [TestCase(typeof(float?), true)]
        [TestCase(null, false)]
        public void Type_IsNullableT(Type typeToCheck, bool expected)
        {
            var result = typeToCheck.IsNullableOfT();
            Assert.AreEqual(expected, result);
        }

        [TestCase(typeof(IEnumerable<float?>), true)]
        [TestCase(typeof(ICollection<float?>), true)]
        [TestCase(typeof(IList<float?>), true)]
        [TestCase(typeof(List<float?>), true)]
        [TestCase(typeof(IEnumerable<float>), false)]
        [TestCase(typeof(ICollection<float>), false)]
        [TestCase(typeof(IList<float>), false)]
        [TestCase(typeof(List<float>), false)]
        public void Type_IsNullableEnumerableT(Type typeToCheck, bool expected)
        {
            var result = typeToCheck.IsNullableEnumerableOfT();
            Assert.AreEqual(expected, result);
        }

        [TestCase(typeof(string), typeof(string))]
        [TestCase(typeof(IEnumerable<string>), typeof(string))]
        [TestCase(typeof(IEnumerable<float?>), typeof(float?))]
        [TestCase(typeof(ICollection<float?>), typeof(float?))]
        [TestCase(typeof(IList<float?>), typeof(float?))]
        [TestCase(typeof(List<float?>), typeof(float?))]
        [TestCase(typeof(IEnumerable<float>), typeof(float))]
        [TestCase(typeof(ICollection<float>), typeof(float))]
        [TestCase(typeof(IList<float>), typeof(float))]
        [TestCase(typeof(List<float>), typeof(float))]
        [TestCase(typeof(List<IEnumerable<float>>), typeof(IEnumerable<float>))]
        [TestCase(typeof(IEnumerable<IEnumerable<IEnumerable<float?>>>), typeof(float?), true)]
        [TestCase(typeof(List<IEnumerable<float>>), typeof(float), true)]
        [TestCase(typeof(List<List<List<List<List<List<float>>>>>>), typeof(float), true)]
        public void Type_GetEnumerableUnderlyingType(Type typeToCheck, Type expected, bool resolveAll = false)
        {
            var result = typeToCheck.GetEnumerableUnderlyingType(resolveAll);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Task_ResultOrDefault_WithNoReturnType_ReturnsNull()
        {
            var task = Task.CompletedTask;
            var result = task.ResultOrDefault();
            Assert.IsNull(result);
        }

        [Test]
        public void Task_ResultOrDefault_WithReturnType_ReturnsObject()
        {
            var task = Task.FromResult(55);
            var result = task.ResultOrDefault();
            Assert.AreEqual(55, result);
        }

        [TestCase(typeof(List<int>), 1)]
        [TestCase(typeof(List<IEnumerable<int>>), 1)]
        [TestCase(typeof(Dictionary<int, string>), 2)]
        [TestCase(typeof(string), 0)]
        public void Type_ExtractGenericParameters(Type type, int expectedCount)
        {
            var paramSet = type.ExtractGenericParameters();
            Assert.AreEqual(expectedCount, paramSet.Count());
        }

        [TestCase(typeof(Dictionary<int, string>), typeof(string))]
        [TestCase(typeof(int), null)]
        [TestCase(typeof(IEnumerable<int>), null)]
        [TestCase(null, null)]
        public void Type_GetValueTypeOfDictionary(Type typeToCheck, Type expectedResult)
        {
            var type = typeToCheck.GetValueTypeOfDictionary();
            Assert.AreEqual(expectedResult, type);
        }

        [TestCase(typeof(Dictionary<int, string>), typeof(int))]
        [TestCase(typeof(int), null)]
        [TestCase(typeof(IEnumerable<int>), null)]
        [TestCase(null, null)]
        public void Type_GetKeyTypeOfDictionary(Type typeToCheck, Type expectedResult)
        {
            var type = typeToCheck.GetKeyTypeOfDictionary();
            Assert.AreEqual(expectedResult, type);
        }

        [Test]
        public async Task UnWrapException_EnsureUnwrap()
        {
            var failedTask = Task.FromException(new Exception("fail task"));
            await Task.Delay(1);
            var exception = failedTask.UnwrapException();

            Assert.IsNotNull(exception);
            Assert.AreEqual("fail task", exception.Message);
        }

        [Test]
        public async Task UnWrapException_NotFaultedTask_ReturnsNUll()
        {
            var failedTask = Task.CompletedTask;
            await Task.Delay(1);
            var exception = failedTask.UnwrapException();

            Assert.IsNull(exception);
        }
    }
}