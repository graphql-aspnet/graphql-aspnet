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
            var result = invoker(instance, 5, 3);
            Assert.AreEqual(8, result);

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
            var instance = new InstanceFactoryTests();
            invoker(instance, 13);
            Assert.AreEqual(13, instance.SettableNumber);

            // ensure it was cached.
            Assert.AreEqual(1, InstanceFactory.PropertyInvokers.Count);
            InstanceFactory.Clear();
        }
    }
}