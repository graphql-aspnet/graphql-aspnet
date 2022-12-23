// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Engine
{
    using System;
    using System.Collections.Generic;
    using GraphQL.AspNet;
    using GraphQL.AspNet.Engine;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Tests.Engine.DefaultScalarTypeProviderTestData;
    using NUnit.Framework;

    [TestFixture]
    public class DefaultScalarTypeProviderTests
    {
        [Test]
        public void EachInvocationOfaScalarIsANewInstance()
        {
            var provider = new DefaultScalarGraphTypeProvider();
            var instances = new List<object>();

            for (var i = 0; i < 3; i++)
            {
                var scalar = provider.CreateScalar(typeof(int));
                instances.Add(scalar);
            }

            Assert.AreEqual(3, instances.Count);

            Assert.IsFalse(ReferenceEquals(instances[0], instances[1]));
            Assert.IsFalse(ReferenceEquals(instances[0], instances[2]));
            Assert.IsFalse(ReferenceEquals(instances[1], instances[2]));
        }

        [Test]
        public void AllRegisteredTypesProduceSameScalar()
        {
            var provider = new DefaultScalarGraphTypeProvider();

            var primary = provider.CreateScalar(typeof(int));
            var secondary = provider.CreateScalar(typeof(int?));

            Assert.IsFalse(ReferenceEquals(primary, secondary));
            Assert.AreEqual(primary.Name, secondary.Name);
        }

        [Test]
        public void IsLeaf_NullType_IsFalse()
        {
            var provider = new DefaultScalarGraphTypeProvider();

            var result = provider.IsLeaf(null);
            Assert.IsFalse(result);
        }

        [Test]
        public void RegisterCustomScalar_ValidScalarIsRegistered()
        {
            var provider = new DefaultScalarGraphTypeProvider();

            provider.RegisterCustomScalar(typeof(ScalarFullyValid));

            var instance = provider.CreateScalar(typeof(ScalarDataType));

            Assert.IsNotNull(instance);
            Assert.AreEqual(typeof(ScalarDataType), instance.ObjectType);
        }

        [Test]
        public void AllDefaultScalars_CanBeInstantiatedAndSearched()
        {
            var provider = new DefaultScalarGraphTypeProvider();

            foreach (var instanceType in provider.ConcreteTypes)
            {
                var instanceFromConcrete = provider.CreateScalar(instanceType);
                Assert.IsNotNull(instanceFromConcrete, $"Could not create scalar from type '{instanceType.Name}'");

                var instanceFromName = provider.CreateScalar(instanceFromConcrete.Name);
                Assert.IsNotNull(instanceFromName, $"Could not create scalar from name '{instanceFromConcrete.Name}'");
            }
        }

        [Test]
        public void CreateScalar_ForUnRegisteredScalarName_ReturnsNull()
        {
            var provider = new DefaultScalarGraphTypeProvider();
            var instance = provider.CreateScalar("NotAScalarName");

            Assert.IsNull(instance);
        }

        [Test]
        public void CreateScalar_ForUnRegisteredScalarType_ReturnsNull()
        {
            var provider = new DefaultScalarGraphTypeProvider();
            var instance = provider.CreateScalar(typeof(DefaultScalarTypeProviderTests));

            Assert.IsNull(instance);
        }

        [Test]
        public void RetrieveConcreteType_ByScalarName_ReturnsScalar()
        {
            var provider = new DefaultScalarGraphTypeProvider();
            var type = provider.RetrieveConcreteType(Constants.ScalarNames.INT);

            Assert.AreEqual(typeof(int), type);
        }

        [Test]
        public void RetrieveConcreteType_ByScalarName_ReturnsNull()
        {
            var provider = new DefaultScalarGraphTypeProvider();
            var type = provider.RetrieveConcreteType("not a scalar name");

            Assert.IsNull(type);
        }

        [Test]
        public void RetrieveScalarName_ByConcreteType_ReturnsnUll()
        {
            var provider = new DefaultScalarGraphTypeProvider();
            var type = provider.RetrieveScalarName(typeof(DefaultScalarTypeProviderTests));

            Assert.IsNull(type);
        }

        [Test]
        public void RetrieveScalarName_ByConcreteType()
        {
            var provider = new DefaultScalarGraphTypeProvider();
            var name = provider.RetrieveScalarName(typeof(int));

            Assert.AreEqual(Constants.ScalarNames.INT, name);
        }

        [TestCase(null, typeof(ArgumentNullException))]
        [TestCase(typeof(ScalarTypeDoesNotImplementScalarInterface), typeof(GraphTypeDeclarationException))]
        [TestCase(typeof(ScalarNoDefaultConstructor), typeof(GraphTypeDeclarationException))]
        [TestCase(typeof(ScalarInvalidGraphName), typeof(GraphTypeDeclarationException))]
        [TestCase(typeof(ScalarNoGraphName), typeof(GraphTypeDeclarationException))]
        [TestCase(typeof(ScalarInUseGraphName), typeof(GraphTypeDeclarationException))]
        [TestCase(typeof(ScalarNotCorrectTypeKind), typeof(GraphTypeDeclarationException))]
        [TestCase(typeof(ScalarNoPrimaryObjectType), typeof(GraphTypeDeclarationException))]
        [TestCase(typeof(ScalarNoSourceResolver), typeof(GraphTypeDeclarationException))]
        [TestCase(typeof(ScalarUnknownValueType), typeof(GraphTypeDeclarationException))]
        [TestCase(typeof(ScalarNullOtherTypeCollection), typeof(GraphTypeDeclarationException))]
        [TestCase(typeof(ScalarNullAppliedDirectives), typeof(GraphTypeDeclarationException))]
        [TestCase(typeof(ScalarIncorrectAppliedDirectivesParent), typeof(GraphTypeDeclarationException))]
        [TestCase(typeof(ScalarImplementationTypeInUse), typeof(GraphTypeDeclarationException))]
        [TestCase(typeof(ScalarOtherTypeInUse), typeof(GraphTypeDeclarationException))]
        public void RegisterCustomScalar_ExpectedDeclarationException(Type scalarType, Type expectedExceptionType)
        {
            var provider = new DefaultScalarGraphTypeProvider();
            try
            {
                provider.RegisterCustomScalar(scalarType);
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.GetType() == expectedExceptionType, $"Expected exception type not thrown. Tested Type '{scalarType?.Name ?? "-null-"}'");
                return;
            }

            Assert.Fail($"Excepted an exception of type '{expectedExceptionType.Name}' to be thrown.");
        }
    }
}