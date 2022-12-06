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
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Tests.Common.ValidationTestData;
    using NUnit.Framework;

    [TestFixture]
    public class ValidationClassTests
    {
        [TestCase(typeof(int?), true)]
        [TestCase(typeof(int), false)]
        [TestCase(typeof(DateTime?), true)]
        [TestCase(typeof(DateTime), false)]
        [TestCase(typeof(string), false)]
        [TestCase(typeof(ValidationClassTests), false)]
        public void IsNullableOfT(Type typeToCheck, bool isSuccess)
        {
            Assert.AreEqual(isSuccess, Validation.IsNullableOfT(typeToCheck));
        }

        [TestCase(null, true)]
        [TestCase(123, false)]
        public void ThrowIfNull(object item, bool shouldThrow)
        {
            if (shouldThrow)
            {
                Assert.Throws<ArgumentNullException>(() =>
                {
                    Validation.ThrowIfNull(item, nameof(item));
                });
            }
            else
            {
                Validation.ThrowIfNull(item, nameof(item));
            }
        }

        [TestCase(null, true)]
        [TestCase(123, false)]
        public void ThrowIfNullOrReturn(object item, bool shouldThrow)
        {
            if (shouldThrow)
            {
                Assert.Throws<ArgumentNullException>(() =>
                {
                    Validation.ThrowIfNullOrReturn(item, nameof(item));
                });
            }
            else
            {
                var output = Validation.ThrowIfNullOrReturn(item, nameof(item));
                Assert.AreEqual(item, output);
            }
        }

        [TestCase(-1, true)]
        [TestCase(0, true)]
        [TestCase(1, false)]
        [TestCase(2, false)]
        public void ThrowIfLessThan1(int item, bool shouldThrow)
        {
            if (shouldThrow)
            {
                Assert.Throws<ArgumentException>(() =>
                {
                    Validation.ThrowIfLessThanOne(item, nameof(item));
                });
            }
            else
            {
                Validation.ThrowIfLessThanOne(item, nameof(item));
            }
        }

        [TestCase(-1, true)]
        [TestCase(0, true)]
        [TestCase(1, false)]
        [TestCase(2, false)]
        public void ThrowIfLessThanOneOrReturn(int item, bool shouldThrow)
        {
            if (shouldThrow)
            {
                Assert.Throws<ArgumentException>(() =>
                {
                    Validation.ThrowIfLessThanOneOrReturn(item, nameof(item));
                });
            }
            else
            {
                var output = Validation.ThrowIfLessThanOneOrReturn(item, nameof(item));
                Assert.AreEqual(item, output);
            }
        }

        [TestCase(typeof(ValidationTestEnum), true)]
        [TestCase(typeof(ValidationClassTests), false)]
        [TestCase(null, false)]
        public void IsEnumeration(Type type, bool isSuccess)
        {
            Assert.AreEqual(isSuccess, Validation.IsEnumeration(type));
        }

        [TestCase(typeof(ObjectChainTests.BisA), typeof(ObjectChainTests.A), true)]
        [TestCase(typeof(ObjectChainTests.CIsNotA), typeof(ObjectChainTests.A), false)]
        [TestCase(typeof(ObjectChainTests.DisB), typeof(ObjectChainTests.A), true)]
        [TestCase(null, typeof(ObjectChainTests.A), false)]
        [TestCase(typeof(ObjectChainTests.DisB), null, false)]
        public void IsCastable(Type typeToCheck, Type typeToCastTo, bool isSuccessful)
        {
            Assert.AreEqual(isSuccessful, Validation.IsCastable(typeToCheck, typeToCastTo));
        }

        [Test]
        public void IsCastable_AsGeneric()
        {
            Assert.IsTrue(Validation.IsCastable<ObjectChainTests.A>(typeof(ObjectChainTests.BisA)));
        }

        [Test]
        public void ThrowIfNotCastable_ThrowsWhenNotCastable()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                Validation.ThrowIfNotCastable<ObjectChainTests.A>(typeof(ObjectChainTests.CIsNotA), "fakeName");
            });
        }

        [Test]
        public void ThrowIfNotCastable_DoesNotThrowWhenCastable()
        {
            Validation.ThrowIfNotCastable<ObjectChainTests.A>(typeof(ObjectChainTests.BisA), "fakeName");
        }

        [Test]
        public void IsCastableAsGeneric()
        {
            Assert.IsTrue(Validation.IsCastable<ObjectChainTests.A>(typeof(ObjectChainTests.BisA)));
        }

        [TestCase("bob", false)]
        [TestCase("bob  ", false)]
        [TestCase("", true)]
        [TestCase(null, true)]
        [TestCase("\t     \n\r", true)]
        public void ThrowIfNullWhiteSpace(string test, bool shouldThrow)
        {
            if (shouldThrow)
            {
                Assert.Throws<ArgumentException>(() =>
                {
                    Validation.ThrowIfNullWhiteSpace(test, nameof(test));
                });
            }
            else
            {
                Validation.ThrowIfNullWhiteSpace(test, nameof(test));
            }
        }

        [TestCase("bob", true, false, "bob")]
        [TestCase("bob  ", true, false, "bob")]
        [TestCase("bob  ", false, false, "bob  ")]
        [TestCase("  b ob  ", true, false, "b ob")]
        [TestCase("  b ob  ", false, false, "  b ob  ")]
        [TestCase("", true, true, null)]
        [TestCase(null, true, true, null)]
        [TestCase("\t     \n\r", true, true, null)]
        public void ThrowIfNullWhiteSpaceOrReturn(string test, bool shouldTrim, bool shouldThrow, string expectedOutput)
        {
            if (shouldThrow)
            {
                Assert.Throws<ArgumentException>(() =>
                {
                    Validation.ThrowIfNullWhiteSpaceOrReturn(test, nameof(test), shouldTrim);
                });
            }
            else
            {
                var result = Validation.ThrowIfNullWhiteSpaceOrReturn(test, nameof(test), shouldTrim);
                Assert.AreEqual(expectedOutput, result);
            }
        }

        [TestCase("bob", false)]
        [TestCase("bob  ", false)]
        [TestCase("", true)]
        [TestCase(null, true)]
        [TestCase("\t     \n\r", false)]
        public void ThrowIfNullEmpty(string test, bool shouldThrow)
        {
            if (shouldThrow)
            {
                Assert.Throws<ArgumentException>(() =>
                {
                    Validation.ThrowIfNullEmpty(test, nameof(test));
                });
            }
            else
            {
                Validation.ThrowIfNullEmpty(test, nameof(test));
            }
        }

        [TestCase("bob", true, false, "bob")]
        [TestCase("bob  ", true, false, "bob")]
        [TestCase("bob  ", false, false, "bob  ")]
        [TestCase("", true, true, null)]
        [TestCase(null, true, true, null)]
        [TestCase("\t     \n\r", true, false, "")]
        [TestCase("\t     \n\r", false, false, "\t     \n\r")]
        public void ThrowIfNullEmptyOrReturn(string test, bool shouldTrim, bool shouldThrow, string expectedOutput)
        {
            if (shouldThrow)
            {
                Assert.Throws<ArgumentException>(() =>
                {
                    Validation.ThrowIfNullEmptyOrReturn(test, nameof(test), shouldTrim);
                });
            }
            else
            {
                var result = Validation.ThrowIfNullEmptyOrReturn(test, nameof(test), shouldTrim);
                Assert.AreEqual(expectedOutput, result);
            }
        }

        [TestCase(null, true)]
        [TestCase(typeof(ValidationClassTests), true)]
        [TestCase(typeof(ValidationTestEnum), false)]
        public void ThrowIfNullOrNotEnumOrReturn(Type typeToCheck, bool shouldThrow)
        {
            if (shouldThrow)
            {
                if (typeToCheck == null)
                {
                    Assert.Throws<ArgumentNullException>(() =>
                    {
                        Validation.ThrowIfNullOrNotEnumOrReturn(typeToCheck, nameof(typeToCheck));
                    });
                }
                else
                {
                    Assert.Throws<ArgumentException>(() =>
                    {
                        Validation.ThrowIfNullOrNotEnumOrReturn(typeToCheck, nameof(typeToCheck));
                    });
                }
            }
            else
            {
                var result = Validation.ThrowIfNullOrNotEnumOrReturn(typeToCheck, nameof(typeToCheck));
                Assert.AreEqual(typeToCheck, result);
            }
        }
    }
}