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
    using GraphQL.AspNet.Common.Generics;
    using GraphQL.AspNet.Tests.Common.InstanceFactoryData;
    using NUnit.Framework;

    [TestFixture]
    public class InstanceFactoryTests
    {
        public int AddNumbers(int arg1, int arg2)
        {
            return arg1 + arg2;
        }

        public void DivideNumbers(int arg1, int arg2)
        {
        }

        public static int SubtractNumbers(int arg1, int arg2)
        {
            return arg1 - arg2;
        }

        public int SettableNumber { get; set; }

        public int GettableOnlyNumber { get; }

        [Test]
        public void ObjectActivator_EmptyConstructor_CreatesObject_CachesActivator()
        {
            InstanceFactory.Clear();

            var instance = InstanceFactory.CreateInstance(typeof(ConstructableObject), null);
            Assert.IsNotNull(instance);

            Assert.AreEqual(1, InstanceFactory.ObjectCreators.Count);

            InstanceFactory.Clear();
        }

        [Test]
        public void ObjectActivator_ViaGeneric_EmptyConstructor_CreatesObject_CachesActivator()
        {
            InstanceFactory.Clear();

            var instance = InstanceFactory.CreateInstance<ConstructableObject>(null);
            Assert.IsNotNull(instance);

            Assert.AreEqual(1, InstanceFactory.ObjectCreators.Count);

            InstanceFactory.Clear();
        }

        [Test]
        public void ObjectActivator_SupplingOneArgument_CreatesObject_CachesActivator()
        {
            InstanceFactory.Clear();

            var instance = InstanceFactory.CreateInstance(typeof(ConstructableObject), "bob");
            Assert.IsNotNull(instance);

            Assert.AreEqual(1, InstanceFactory.ObjectCreators.Count);

            InstanceFactory.Clear();
        }

        [Test]
        public void ObjectActivator_SupplingFourArguments_CreatesObject_DoesNOTCacheActivator()
        {
            InstanceFactory.Clear();

            var instance = InstanceFactory.CreateInstance(typeof(ConstructableObject), "bob1", "bob2", "bob3", "bob4");
            Assert.IsNotNull(instance);

            Assert.AreEqual(0, InstanceFactory.ObjectCreators.Count);
        }

        [Test]
        public void ObjectActivator_ConstructorDoesNotExist_ThrowsException()
        {
            InstanceFactory.Clear();

            Assert.Throws<InvalidOperationException>(() => InstanceFactory.CreateInstance(typeof(ConstructableObject), 5));
            Assert.AreEqual(0, InstanceFactory.ObjectCreators.Count);
        }

        [Test]
        public void ObjectActivator_Struct_EmptyConstructor_CreatesObject()
        {
            InstanceFactory.Clear();

            // all structs have an empty constructor, even if not declared
            var instance = InstanceFactory.CreateInstance(typeof(NonParamerizedStruct));
            Assert.IsNotNull(instance);

            var castedItem = (NonParamerizedStruct)instance;
            Assert.AreEqual(null, castedItem.Property1);
        }

        [Test]
        public void ObjectActivator_Struct_ParameterizedConstructor_CreatesObject()
        {
            InstanceFactory.Clear();

            var instance = InstanceFactory.CreateInstance(typeof(ParameterizedStruct), "aValueForProp1");
            Assert.IsNotNull(instance);

            var castedItem = (ParameterizedStruct)instance;
            Assert.AreEqual("aValueForProp1", castedItem.Property1);
        }

        [Test]
        public void ObjectActivator_Struct_NonParameterizedConstructorOfStructWithParameterizedConstructor_CreatesObject()
        {
            InstanceFactory.Clear();

            // the internal empty ctor should still be callable
            var instance = InstanceFactory.CreateInstance(typeof(ParameterizedStruct));
            Assert.IsNotNull(instance);

            var castedItem = (ParameterizedStruct)instance;
            Assert.AreEqual(null, castedItem.Property1);
        }

        [Test]
        public void MethodInvoker_NullMethodInfo_returnsNull()
        {
            var invoker = InstanceFactory.CreateInstanceMethodInvoker(null);
            Assert.IsNull(invoker);
        }

        [Test]
        public void MethodInvoker_StaticMethodInfo_ThrowsException()
        {
            InstanceFactory.Clear();
            var methodInfo = typeof(InstanceFactoryTests).GetMethod(nameof(InstanceFactoryTests.SubtractNumbers));

            Assert.Throws<ArgumentException>(() =>
            {
                var invoker = InstanceFactory.CreateInstanceMethodInvoker(methodInfo);
            });
        }

        [Test]
        public void MethodInvoker_VoidReturn_ThrowsException()
        {
            InstanceFactory.Clear();
            var methodInfo = typeof(InstanceFactoryTests).GetMethod(nameof(InstanceFactoryTests.DivideNumbers));

            Assert.Throws<ArgumentException>(() =>
            {
                var invoker = InstanceFactory.CreateInstanceMethodInvoker(methodInfo);
            });
        }

        [Test]
        public void MethodInvoker_StandardInvoke_ReturnsValue()
        {
            InstanceFactory.Clear();
            var methodInfo = typeof(InstanceFactoryTests).GetMethod(nameof(InstanceFactoryTests.AddNumbers));

            var invoker = InstanceFactory.CreateInstanceMethodInvoker(methodInfo);

            var instance = new InstanceFactoryTests();
            var obj = instance as object;
            var result = invoker(ref obj, 5, 3);
            Assert.AreEqual(8, result);

            // ensure it was cached.
            Assert.AreEqual(1, InstanceFactory.MethodInvokers.Count);
            InstanceFactory.Clear();
        }

        [Test]
        public void MethodInvoker_Struct_StandardInvoke_ReturnsValue_StructIsModified()
        {
            InstanceFactory.Clear();
            var methodInfo = typeof(StructWithMethod).GetMethod(nameof(StructWithMethod.AddAndSet));

            var invoker = InstanceFactory.CreateInstanceMethodInvoker(methodInfo);

            var instance = default(StructWithMethod);
            var obj = (object)instance;
            var result = invoker(ref obj, "propValue1", 3);

            var resultCast = (StructWithMethod)obj;

            Assert.AreEqual(4, result);
            Assert.AreEqual("propValue1", resultCast.Property1);

            // ensure it was cached.
            Assert.AreEqual(1, InstanceFactory.MethodInvokers.Count);
            InstanceFactory.Clear();
        }

        [Test]
        public void PropertySetterInvoker_StandardInvoke_ReturnsValue()
        {
            InstanceFactory.Clear();

            var invokerSet = InstanceFactory.CreatePropertySetterInvokerCollection(typeof(InstanceFactoryTests));

            // ensure the "gettable" only property was skipped
            Assert.AreEqual(1, invokerSet.Count);

            invokerSet.TryGetValue(nameof(SettableNumber), out var invoker);

            Assert.IsNotNull(invoker);
            var instance = (object)new InstanceFactoryTests();
            invoker(ref instance, 13);

            var recast = instance as InstanceFactoryTests;
            Assert.AreEqual(13, recast.SettableNumber);

            // ensure it was cached.
            Assert.AreEqual(1, InstanceFactory.PropertyInvokers.Count);
            InstanceFactory.Clear();
        }

        [Test]
        public void PropertySetterInvoker_Struct_StandardInvoke_ReturnsValue()
        {
            InstanceFactory.Clear();

            var invokerSet = InstanceFactory.CreatePropertySetterInvokerCollection(typeof(NonParamerizedStruct));

            // ensure the "gettable" only property was skipped
            Assert.AreEqual(1, invokerSet.Count);

            invokerSet.TryGetValue(nameof(NonParamerizedStruct.Property1), out var invoker);

            Assert.IsNotNull(invoker);
            var instance = default(NonParamerizedStruct);
            var obj = (object)instance;
            invoker(ref obj, "prop1Value");

            var recast = (NonParamerizedStruct)obj;
            Assert.AreEqual("prop1Value", recast.Property1);

            // ensure it was cached.
            Assert.AreEqual(1, InstanceFactory.PropertyInvokers.Count);
            InstanceFactory.Clear();
        }
    }
}