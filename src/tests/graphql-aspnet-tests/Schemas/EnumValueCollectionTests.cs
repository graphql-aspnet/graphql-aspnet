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
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.Structural;
    using Moq;
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
            var owner = new Mock<IEnumGraphType>();
            owner.Setup(x => x.Name).Returns("graphType");
            owner.Setup(x => x.ObjectType).Returns(typeof(EnumValueTestEnum));

            var enumValue = new Mock<IEnumValue>();
            enumValue.Setup(x => x.Name).Returns("VALUE1");
            enumValue.Setup(x => x.InternalValue).Returns(EnumValueTestEnum.Value1);
            enumValue.Setup(x => x.InternalLabel).Returns(EnumValueTestEnum.Value1.ToString());

            var enumValue2 = new Mock<IEnumValue>();
            enumValue2.Setup(x => x.Name).Returns("VALUE1");
            enumValue2.Setup(x => x.InternalValue).Returns(EnumValueTestEnum.Value1);
            enumValue.Setup(x => x.InternalLabel).Returns(EnumValueTestEnum.Value1.ToString());

            var collection = new EnumValueCollection(owner.Object);
            collection.Add(enumValue.Object);

            Assert.AreEqual(1, collection.Count);
            Assert.AreEqual(1, collection.Keys.Count());
            Assert.AreEqual(1, collection.Values.Count());

            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                collection.Add(enumValue2.Object);
            });
        }

        [TestCase(EnumValueTestEnum.Value1, true, true)]
        [TestCase((EnumValueTestEnum)0, false, false)]
        [TestCase(EnumValueTestEnum.Value2, true, false)]
        public void FindEnumValue(EnumValueTestEnum testValue, bool doesValidate, bool shouldBeFound)
        {
            var owner = new Mock<IEnumGraphType>();
            owner.Setup(x => x.Name).Returns("graphType");
            owner.Setup(x => x.ValidateObject(It.IsAny<object>())).Returns(doesValidate);
            owner.Setup(x => x.ObjectType).Returns(typeof(EnumValueTestEnum));

            var enumValue = new Mock<IEnumValue>();
            enumValue.Setup(x => x.Name).Returns("VALUE1");
            enumValue.Setup(x => x.InternalValue).Returns(EnumValueTestEnum.Value1);
            enumValue.Setup(x => x.InternalLabel).Returns(EnumValueTestEnum.Value1.ToString());

            var collection = new EnumValueCollection(owner.Object);
            collection.Add(enumValue.Object);

            var result = collection.FindByEnumValue(testValue);

            if (shouldBeFound)
                Assert.AreEqual(enumValue.Object, result);
            else
                Assert.IsNull(result);
        }

        [TestCase("VALUE1", true)]
        [TestCase("ValUE1", true)] // not case sensitive
        [TestCase("VALUE2", false)]
        public void ContainsKey(string testValue, bool shouldBeFound)
        {
            var owner = new Mock<IEnumGraphType>();
            owner.Setup(x => x.Name).Returns("graphType");
            owner.Setup(x => x.ObjectType).Returns(typeof(EnumValueTestEnum));

            var enumValue = new Mock<IEnumValue>();
            enumValue.Setup(x => x.Name).Returns("VALUE1");
            enumValue.Setup(x => x.InternalValue).Returns(EnumValueTestEnum.Value1);
            enumValue.Setup(x => x.InternalLabel).Returns(EnumValueTestEnum.Value1.ToString());

            var collection = new EnumValueCollection(owner.Object);
            collection.Add(enumValue.Object);

            var result = collection.ContainsKey(testValue);

            Assert.AreEqual(shouldBeFound, result);
        }

        [TestCase("VALUE1", true)]
        [TestCase("VALUE2", false)]
        public void ThisByName(string name, bool shouldBeFound)
        {
            var owner = new Mock<IEnumGraphType>();
            owner.Setup(x => x.Name).Returns("graphType");
            owner.Setup(x => x.ObjectType).Returns(typeof(EnumValueTestEnum));

            var enumValue = new Mock<IEnumValue>();
            enumValue.Setup(x => x.Name).Returns("VALUE1");
            enumValue.Setup(x => x.InternalValue).Returns(EnumValueTestEnum.Value1);
            enumValue.Setup(x => x.InternalLabel).Returns(EnumValueTestEnum.Value1.ToString());

            var collection = new EnumValueCollection(owner.Object);
            collection.Add(enumValue.Object);

            if (shouldBeFound)
            {
                var result = collection[name];
                Assert.AreEqual(enumValue.Object, result);
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
    }
}