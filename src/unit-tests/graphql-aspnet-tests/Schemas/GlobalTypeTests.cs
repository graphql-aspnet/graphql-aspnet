// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Schemas
{
    using System;
    using System.Linq;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Schemas.TypeSystem.Scalars;
    using GraphQL.AspNet.Tests.Common.CommonHelpers;
    using GraphQL.AspNet.Tests.Schemas.GlobalTypesTestData;
    using Microsoft.AspNetCore.Mvc.ApplicationParts;
    using NUnit.Framework;

    public class GlobalTypeTests
    {
        [Test]
        public void AllInternalScalarsExistInGlobalCollection()
        {
            // sanity check to ensure wireups are correct
            var types = typeof(IntScalarType).Assembly
                .GetTypes()
                .Where(x => Validation.IsCastable<IScalarGraphType>(x)
                && !x.IsAbstract
                && x.FullName.StartsWith("GraphQL.AspNet.Schemas.TypeSystem.Scalars"));

            foreach (var type in types)
            {
                Assert.IsTrue(GlobalTypes.IsBuiltInScalar(type), $"{type.Name} is not declared as built in but should be");
            }
        }

        [TestCase("Int", true)]
        [TestCase("int", true)] // test for case-insensitiveness
        [TestCase("INT", true)]
        [TestCase("Float", true)]
        [TestCase("Boolean", true)]
        [TestCase("String", true)]
        [TestCase("Id", true)]
        [TestCase("Long", true)]
        [TestCase("UInt", true)]
        [TestCase("ULong", true)]
        [TestCase("Double", true)]
        [TestCase("Decimal", true)]
        [TestCase("DateTimeOffset", true)]
        [TestCase("DateTime", true)]
        [TestCase("DateOnly", true)]
        [TestCase("TimeOnly", true)]
        [TestCase("Byte", true)]
        [TestCase("SignedByte", true)]
        [TestCase("Short", true)]
        [TestCase("UShort", true)]
        [TestCase("Guid", true)]
        [TestCase("Uri", true)]

        [TestCase("NotAScalar", false)]
        [TestCase(null, false)]
        public static void BuiltInScalarNames(string name, bool isBuiltIn)
        {
            Assert.AreEqual(isBuiltIn, GlobalTypes.IsBuiltInScalar(name));
        }

        [TestCase("Int", false)]
        [TestCase("Float", false)]
        [TestCase("Boolean", false)]
        [TestCase("String", false)]
        [TestCase("Id", false)]
        [TestCase("Long", true)]
        [TestCase("UInt", true)]
        [TestCase("ULong", true)]
        [TestCase("Double", true)]
        [TestCase("Decimal", true)]
        [TestCase("DateTimeOffset", true)]
        [TestCase("DateTime", true)]
        [TestCase("DateOnly", true)]
        [TestCase("TimeOnly", true)]
        [TestCase("Byte", true)]
        [TestCase("SignedByte", true)]
        [TestCase("Short", true)]
        [TestCase("UShort", true)]
        [TestCase("Guid", true)]
        [TestCase("Uri", true)]
        [TestCase(null, true)]
        public static void CanBeRenamed(string name, bool canBeRenamed)
        {
            Assert.AreEqual(canBeRenamed, GlobalTypes.CanBeRenamed(name));
        }

        [TestCase(typeof(int), typeof(IntScalarType))]
        [TestCase(typeof(TwoPropertyObject), null)]
        [TestCase(null, null)]
        public static void FindBuiltInScalarType(Type typeToTest, Type expectedOutput)
        {
            var result = GlobalTypes.FindBuiltInScalarType(typeToTest);
            Assert.AreEqual(expectedOutput, result);
        }

        [TestCase(typeof(IntScalarType), false)]
        [TestCase(null, true)] // no type provided
        [TestCase(typeof(NoParameterlessConstructorScalar), true)]
        [TestCase(typeof(TwoPropertyObject), true)] // does not implement iScalarGraphType
        [TestCase(typeof(InvalidGraphTypeNameScalar), true)]
        [TestCase(typeof(NoNameOnScalar), true)]
        [TestCase(typeof(NoObjectTypeScalar), true)]
        [TestCase(typeof(NotScalarKindScalar), true)]
        [TestCase(typeof(ObjectTypeIsNullableScalar), true)]
        [TestCase(typeof(NoSourceResolverScalar), true)]
        [TestCase(typeof(NoValueTypeScalar), true)]
        [TestCase(typeof(NullAppliedDirectivesScalar), true)]
        [TestCase(typeof(InvalidParentAppliedDirectivesScalar), true)]
        public static void ValidateScalarorThrow(Type typeToTest, bool shouldThrow)
        {
            try
            {
                GlobalTypes.ValidateScalarTypeOrThrow(typeToTest);
            }
            catch (GraphTypeDeclarationException)
            {
                if (shouldThrow)
                    return;

                Assert.Fail("Threw when it shouldnt");
            }
            catch (Exception ex)
            {
                Assert.Fail("Threw invalid exception type");
            }

            if (!shouldThrow)
                return;

            Assert.Fail("Didn't throw when it should");
        }

        [TestCase(typeof(IntScalarType), true)]
        [TestCase(null, false)] // no type provided
        [TestCase(typeof(NoParameterlessConstructorScalar), false)]
        [TestCase(typeof(TwoPropertyObject), false)] // does not implement iScalarGraphType
        [TestCase(typeof(InvalidGraphTypeNameScalar), false)]
        [TestCase(typeof(NoNameOnScalar), false)]
        [TestCase(typeof(NoObjectTypeScalar), false)]
        [TestCase(typeof(NotScalarKindScalar), false)]
        [TestCase(typeof(ObjectTypeIsNullableScalar), false)]
        [TestCase(typeof(NoSourceResolverScalar), false)]
        [TestCase(typeof(NoValueTypeScalar), false)]
        [TestCase(typeof(NullAppliedDirectivesScalar), false)]
        [TestCase(typeof(InvalidParentAppliedDirectivesScalar), false)]
        public static void IsValidScalar(Type typeToTest, bool isValid)
        {
            var result = GlobalTypes.IsValidScalarType(typeToTest);
            Assert.AreEqual(isValid, result);
        }

        [Test]
        public static void ValdidateScalarType_ByScalarInstance_ValidScalar()
        {
            // shoudl not throw
            var instance = new IntScalarType();
            GlobalTypes.ValidateScalarTypeOrThrow(instance);
        }

        [Test]
        public static void ValdidateScalarType_ByScalarInstance_InvalidScalar()
        {
            // shoudl not throw
            var instance = new NoNameOnScalar();
            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                GlobalTypes.ValidateScalarTypeOrThrow(instance);
            });
        }

        [TestCase(null, true)]
        [TestCase(typeof(TwoPropertyObject), true)] // not a union proxy
        [TestCase(typeof(NoParameterelessConstructorProxy), true)]
        [TestCase(typeof(NoNameUnionProxy), true)]
        [TestCase(typeof(InvalidNameUnionProxy), true)]
        [TestCase(typeof(NoUnionMembersProxy), true)]
        [TestCase(typeof(NullTypesUnionProxy), true)]
        [TestCase(typeof(ValidUnionProxy), false)]
        public void ValidateUnionProxyOrThrow(Type typeToTest, bool shouldThrow)
        {
            try
            {
                GlobalTypes.ValidateUnionProxyOrThrow(typeToTest);
            }
            catch (GraphTypeDeclarationException)
            {
                if (shouldThrow)
                    return;

                Assert.Fail("Threw when it shouldnt");
            }
            catch (Exception ex)
            {
                Assert.Fail("Threw invalid exception type");
            }

            if (!shouldThrow)
                return;

            Assert.Fail("Didn't throw when it should");
        }

        [TestCase(null, false)]
        [TestCase(typeof(TwoPropertyObject), false )] // not a union proxy
        [TestCase(typeof(NoParameterelessConstructorProxy), false)]
        [TestCase(typeof(NoNameUnionProxy), false)]
        [TestCase(typeof(InvalidNameUnionProxy), false)]
        [TestCase(typeof(NoUnionMembersProxy), false)]
        [TestCase(typeof(NullTypesUnionProxy), false)]
        [TestCase(typeof(ValidUnionProxy), true)]
        public void IsValidUnionProxyType(Type typeToTest, bool isValid)
        {
            var result = GlobalTypes.IsValidUnionProxyType(typeToTest);
            Assert.AreEqual(isValid, result);
        }

        [Test]
        public void ValidateUnionProxyOrThrow_ByInstance_ValidProxy()
        {
            // should not throw
            GlobalTypes.ValidateUnionProxyOrThrow(new ValidUnionProxy());
        }

        [Test]
        public void ValidateUnionProxyOrThrow_ByInstance_InvalidProxy()
        {
            // should not throw
            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                GlobalTypes.ValidateUnionProxyOrThrow(new NoNameUnionProxy());
            });
        }

        [TestCase(typeof(ValidUnionProxy), true)]
        [TestCase(null, false)]
        [TestCase(typeof(NoNameUnionProxy), false)]
        public void CreateUnionTypeFromProxy(Type proxyType, bool shouldReturnValue)
        {
            var reslt = GlobalTypes.CreateUnionProxyFromType(proxyType);

            if (shouldReturnValue)
                Assert.IsNotNull(reslt);
            else
                Assert.IsNull(reslt);
        }
    }
}