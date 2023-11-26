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
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.Structural;
    using NSubstitute;
    using NUnit.Framework;

    [TestFixture]
    public class EnumValueCollectionTests
    {
        public enum EnumValueTestEnum
        {
            Value1,
            Value2,
        }

        [Test]
        public void Add_DuplicateEnumValueNameThrowsException()
        {
            var owner = Substitute.For<IEnumGraphType>();
            owner.Name.Returns("graphType");
            owner.ObjectType.Returns(typeof(EnumValueTestEnum));

            var enumValue = Substitute.For<IEnumValue>();
            enumValue.Name.Returns("VALUE1");
            enumValue.DeclaredValue.Returns(EnumValueTestEnum.Value1);
            enumValue.DeclaredLabel.Returns(EnumValueTestEnum.Value1.ToString());

            var enumValue2 = Substitute.For<IEnumValue>();
            enumValue2.Name.Returns("VALUE1");
            enumValue2.DeclaredValue.Returns(EnumValueTestEnum.Value1);
            enumValue.DeclaredLabel.Returns(EnumValueTestEnum.Value1.ToString());

            var collection = new EnumValueCollection(owner);
            collection.Add(enumValue);

            Assert.AreEqual(1, collection.Count);
            Assert.AreEqual(1, collection.Keys.Count());
            Assert.AreEqual(1, collection.Values.Count());

            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                collection.Add(enumValue2);
            });
        }

        [TestCase(EnumValueTestEnum.Value1, true, true)]
        [TestCase((EnumValueTestEnum)0, false, false)]
        [TestCase(EnumValueTestEnum.Value2, true, false)]
        public void FindEnumValue(EnumValueTestEnum testValue, bool doesValidate, bool shouldBeFound)
        {
            var owner = Substitute.For<IEnumGraphType>();
            owner.Name.Returns("graphType");
            owner.ValidateObject(Arg.Any<object>()).Returns(doesValidate);
            owner.ObjectType.Returns(typeof(EnumValueTestEnum));

            var enumValue = Substitute.For<IEnumValue>();
            enumValue.Name.Returns("VALUE1");
            enumValue.DeclaredValue.Returns(EnumValueTestEnum.Value1);
            enumValue.DeclaredLabel.Returns(EnumValueTestEnum.Value1.ToString());

            var collection = new EnumValueCollection(owner);
            collection.Add(enumValue);

            var result = collection.FindByEnumValue(testValue);

            if (shouldBeFound)
                Assert.AreEqual(enumValue, result);
            else
                Assert.IsNull(result);
        }

        [Test]
        public void FindEnumValue_NullPassed_RetunsNull()
        {
            var owner = Substitute.For<IEnumGraphType>();
            owner.Name.Returns("graphType");
            owner.ValidateObject(Arg.Any<object>()).Returns(true);
            owner.ObjectType.Returns(typeof(EnumValueTestEnum));

            var enumValue = Substitute.For<IEnumValue>();
            enumValue.Name.Returns("VALUE1");
            enumValue.DeclaredValue.Returns(EnumValueTestEnum.Value1);
            enumValue.DeclaredLabel.Returns(EnumValueTestEnum.Value1.ToString());

            var collection = new EnumValueCollection(owner);
            collection.Add(enumValue);

            var result = collection.FindByEnumValue(null);
            Assert.IsNull(result);
        }

        [TestCase("VALUE1", true)]
        [TestCase("ValUE1", true)] // not case sensitive
        [TestCase("VALUE2", false)]
        public void ContainsKey(string testValue, bool shouldBeFound)
        {
            var owner = Substitute.For<IEnumGraphType>();
            owner.Name.Returns("graphType");
            owner.ObjectType.Returns(typeof(EnumValueTestEnum));

            var enumValue = Substitute.For<IEnumValue>();
            enumValue.Name.Returns("VALUE1");
            enumValue.DeclaredValue.Returns(EnumValueTestEnum.Value1);
            enumValue.DeclaredLabel.Returns(EnumValueTestEnum.Value1.ToString());

            var collection = new EnumValueCollection(owner);
            collection.Add(enumValue);

            var result = collection.ContainsKey(testValue);

            Assert.AreEqual(shouldBeFound, result);
        }

        [TestCase("VALUE1", true)]
        [TestCase("VALUE2", false)]
        public void ThisByName(string name, bool shouldBeFound)
        {
            var owner = Substitute.For<IEnumGraphType>();
            owner.Name.Returns("graphType");
            owner.ObjectType.Returns(typeof(EnumValueTestEnum));

            var enumValue = Substitute.For<IEnumValue>();
            enumValue.Name.Returns("VALUE1");
            enumValue.DeclaredValue.Returns(EnumValueTestEnum.Value1);
            enumValue.DeclaredLabel.Returns(EnumValueTestEnum.Value1.ToString());

            var collection = new EnumValueCollection(owner);
            collection.Add(enumValue);

            if (shouldBeFound)
            {
                var result = collection[name];
                Assert.AreEqual(enumValue, result);
            }
            else
            {
                Assert.Throws(
                    Is.InstanceOf<Exception>(),
                    () =>
                    {
                        var item = collection[name];
                    });
            }
        }

        [TestCase("VALUE1", true)]
        [TestCase("VALUE2", false)]
        public void TryGetValue(string name, bool shouldBeFound)
        {
            var owner = Substitute.For<IEnumGraphType>();
            owner.Name.Returns("graphType");
            owner.ObjectType.Returns(typeof(EnumValueTestEnum));

            var enumValue = Substitute.For<IEnumValue>();
            enumValue.Name.Returns("VALUE1");
            enumValue.DeclaredValue.Returns(EnumValueTestEnum.Value1);
            enumValue.DeclaredLabel.Returns(EnumValueTestEnum.Value1.ToString());

            var collection = new EnumValueCollection(owner);
            collection.Add(enumValue);

            var result = collection.TryGetValue(name, out var item);
            if (shouldBeFound)
            {
                Assert.IsTrue(result);
                Assert.IsNotNull(item);
                Assert.AreEqual(enumValue, item);
            }
            else
            {
                Assert.IsFalse(result);
                Assert.IsNull(item);
            }
        }

        [Test]
        public void Remove_ValidValueIsRemoved()
        {
            var owner = Substitute.For<IEnumGraphType>();
            owner.Name.Returns("graphType");
            owner.ObjectType.Returns(typeof(EnumValueTestEnum));

            var enumValue = Substitute.For<IEnumValue>();
            enumValue.Name.Returns("VALUE1");
            enumValue.DeclaredValue.Returns(EnumValueTestEnum.Value1);
            enumValue.DeclaredLabel.Returns(EnumValueTestEnum.Value1.ToString());

            var collection = new EnumValueCollection(owner);
            collection.Add(enumValue);

            Assert.AreEqual(1, collection.Count);
            collection.Remove("VALUE1");
            Assert.AreEqual(0, collection.Count);
        }

        [Test]
        public void Remove_InvalidValueIsIgnored()
        {
            var owner = Substitute.For<IEnumGraphType>();
            owner.Name.Returns("graphType");
            owner.ObjectType.Returns(typeof(EnumValueTestEnum));

            var enumValue = Substitute.For<IEnumValue>();
            enumValue.Name.Returns("VALUE1");
            enumValue.DeclaredValue.Returns(EnumValueTestEnum.Value1);
            enumValue.DeclaredLabel.Returns(EnumValueTestEnum.Value1.ToString());

            var collection = new EnumValueCollection(owner);
            collection.Add(enumValue);

            Assert.AreEqual(1, collection.Count);
            collection.Remove("VALUE1DDD");
            Assert.AreEqual(1, collection.Count);
        }
    }
}