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
    public class EnumExtensionTestes
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
    }
}