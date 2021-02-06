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
    using System.Linq;
    using GraphQL.AspNet.Common.Extensions;
    using NUnit.Framework;

    [TestFixture]
    public class EnumExtensionTests
    {
        [Flags]
        public enum EnumExtensionTestObject
        {
            None = 0,
            Value1 = 1,
            Value2 = 2,
            Value3 = 4,
            Value4 = Value2 | Value3,
        }

        public enum SingleZeroValueEnumExtensionTestObject
        {
            None = 0,
        }

        public enum EnumSByte : sbyte
        {
            Value1 = (sbyte)1,
        }

        public enum EnumByte : byte
        {
            Value1 = (byte)1,
        }

        public enum EnumShort : short
        {
            Value1 = (short)1,
        }

        public enum EnumUShort : ushort
        {
            Value1 = (ushort)1,
        }

        public enum EnumInt : int
        {
            Value1 = 1,
        }

        public enum EnumUint : uint
        {
            Value1 = 1U,
        }

        public enum EnumLong : long
        {
            Value1 = 1L,
        }

        public enum EnumUlong : ulong
        {
            Value1 = 1UL,
        }

        [Test]
        public void GetFlags_YieldsAllDefinedFlags()
        {
            // returns the defined flags (not the unwrapped flag set)
            var instance = EnumExtensionTestObject.Value1 | EnumExtensionTestObject.Value4;
            var flags = instance.GetFlags<EnumExtensionTestObject>();

            Assert.AreEqual(2, flags.Count());
            Assert.IsTrue(flags.Any(x => x == EnumExtensionTestObject.Value1));
            Assert.IsTrue(flags.Any(x => x == EnumExtensionTestObject.Value4));
        }

        [Test]
        public void GetFlags_ForNonDefinedValue_ReturnsEmptyList()
        {
            var instance = (EnumExtensionTestObject)8;
            var flags = instance.GetFlags<EnumExtensionTestObject>();

            Assert.IsEmpty(flags);
        }

        [Test]
        public void GetFlags_EmptyValue_YieldsEmptyValueInCollection()
        {
            var instance = EnumExtensionTestObject.None;
            var flags = instance.GetFlags<EnumExtensionTestObject>();

            Assert.AreEqual(1, flags.Count());
            Assert.IsTrue(flags.Any(x => x == EnumExtensionTestObject.None));
        }

        [Test]
        public void GetIndividualFlags_YieldsAllDefinedFlagsAndRemovesCompoundFlags()
        {
            // returns the defined flags (and unwrapps Value4)
            var instance = EnumExtensionTestObject.Value1 | EnumExtensionTestObject.Value4;
            var flags = instance.GetIndividualFlags<EnumExtensionTestObject>();

            Assert.AreEqual(3, flags.Count());
            Assert.IsTrue(flags.Any(x => x == EnumExtensionTestObject.Value1));
            Assert.IsTrue(flags.Any(x => x == EnumExtensionTestObject.Value2));
            Assert.IsTrue(flags.Any(x => x == EnumExtensionTestObject.Value3));
            Assert.IsFalse(flags.Any(x => x == EnumExtensionTestObject.Value4));
        }

        [TestCase(typeof(EnumSByte), typeof(sbyte), false)]
        [TestCase(typeof(EnumByte), typeof(byte), true)]
        [TestCase(typeof(EnumShort), typeof(short), false)]
        [TestCase(typeof(EnumUShort), typeof(ushort), true)]
        [TestCase(typeof(EnumInt), typeof(int), false)]
        [TestCase(typeof(EnumUint), typeof(uint), true)]
        [TestCase(typeof(EnumLong), typeof(long), false)]
        [TestCase(typeof(EnumUlong), typeof(ulong), true)]
        public void Enum_IsEnumOfUnsignedNumericType_ReturnsCorrectFlag(
            Type enumType,
            Type expectedUnderlyingType,
            bool expectedUnSigned)
        {
            // pre check to ensure no compiler funny business
            // or type switching
            var underlyingType = Enum.GetUnderlyingType(enumType);
            Assert.AreEqual(expectedUnderlyingType, underlyingType);

            var isUnSigned = enumType.IsEnumOfUnsignedNumericType();
            Assert.AreEqual(expectedUnSigned, isUnSigned);
        }

        [Test]
        public void Enum_IsEnumOfUnsignedNumericType_NonEnumThrowsException()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                typeof(EnumExtensionTests).IsEnumOfUnsignedNumericType();
            });
        }
    }
}