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
    using NUnit.Framework;

    [TestFixture]
    public class GraphIdTests
    {
        [Test]
        public void GeneralPropertyCheck()
        {
            var id = new GraphId("abc");
            Assert.AreEqual("abc", id.Value);
            Assert.AreEqual("abc", id.ToString());
            Assert.AreEqual("abc".GetHashCode(), id.GetHashCode());
        }

        [Test]
        public void StringExplicitCast_IsSuccessful()
        {
            GraphId id = (GraphId)"abc";

            Assert.AreEqual("abc", id.Value);
        }

        [Test]
        public void StringImplicitCast_IsSuccessful()
        {
            GraphId id = new GraphId("abc");
            string str = id;

            Assert.AreEqual("abc", str);
        }

        [Test]
        public void ConstructorFromGraphId_YieldsIdValueThatMatches()
        {
            var id = new GraphId("abc");

            var newId = new GraphId(id);
            Assert.AreEqual(id.Value, newId.Value);
        }

        [Test]
        public void EqualsMethodAgainstNonString_DoesNotMatch()
        {
            var id = new GraphId("abc");

            var idValue = (object)123;
            Assert.IsFalse(id.Equals(idValue));
        }

        [Test]
        public void EqualsMethodAgainstNull_DoesNotMatch()
        {
            var id = new GraphId("abc");
            Assert.IsFalse(id.Equals(null as object));
        }

        [Test]
        public void EqualsMethodAgainstNullAsObject_DoesNotMatch()
        {
            var id = new GraphId("abc");
            Assert.IsFalse(id.Equals(null));
        }

        [Test]
        public void EqualsMethodAgainstSelf_Matches()
        {
            var id = new GraphId("abc");
            Assert.IsTrue(id.Equals(id));
        }

        [Test]
        public void EqualsMethodAgainstGraphId_Matches()
        {
            var id = new GraphId("abc");
            var id2 = new GraphId("abc");

            Assert.IsTrue(id.Equals(id2));
        }

        [Test]
        public void EqualsMethodAgainstSelfAsObject_Matches()
        {
            var id = new GraphId("abc");

            Assert.IsTrue(id.Equals((object)id));
        }

        [Test]
        public void EqualsMethodAgainstOtherIdAsObject_Matches()
        {
            var id = new GraphId("abc");
            var secondId = new GraphId("abc");

            Assert.IsTrue(id.Equals((object)secondId));
        }

        [Test]
        public void EqualsMethodAgainstStringObject_Matches()
        {
            var id = new GraphId("abc");

            var obj = (object)"abc";
            Assert.IsTrue(id.Equals(obj));
        }

        [Test]
        public void EmptyConstructorHasNullValue()
        {
            var id = new GraphId(null);
            Assert.AreEqual(null, id.Value);
        }

        [TestCase("123", "123", true)]
        [TestCase(null, null, true)]
        [TestCase("", "", true)]
        [TestCase("123", null, false)]
        [TestCase(null, "123", false)]
        [TestCase("", null, false)]
        [TestCase("123", "345", false)]
        public void EqualsOperator(string value1, string value2, bool isSuccess)
        {
            var id1 = new GraphId(value1);
            var id2 = new GraphId(value2);

            Assert.AreEqual(isSuccess, id1 == id2);
            Assert.AreEqual(isSuccess, id2 == id1);
        }

        [Test]
        public void EqualsOperatorAgainstSelf_IsTrue()
        {
            var id = new GraphId("123");

            // ReSharper disable once EqualExpressionComparison
#pragma warning disable CS1718 // Comparison made to same variable
            Assert.IsTrue(id == id);
#pragma warning restore CS1718 // Comparison made to same variable
        }

        [Test]
        public void EqualsOperatorWithNullValueAgaintNullObject_IsFalse()
        {
            var id = new GraphId(null);

            object obj = null;
            Assert.IsFalse(id == obj);
        }

        [TestCase("123", "123", true)]
        [TestCase("123", 123, false)]
        [TestCase("123", null, false)]
        public void EqualsOperator_AgainstObject(string value1, object value2, bool isSuccess)
        {
            var id1 = new GraphId(value1);

            Assert.AreEqual(isSuccess, id1 == value2);
            Assert.AreEqual(isSuccess, value2 == id1);
        }

        [TestCase("123", "123", false)]
        [TestCase(null, null, false)]
        [TestCase("", "", false)]
        [TestCase("123", null, true)]
        [TestCase(null, "123", true)]
        [TestCase("", null, true)]
        [TestCase("123", "345", true)]
        public void NotEqualsOperator(string value1, string value2, bool isSuccess)
        {
            var id1 = new GraphId(value1);
            var id2 = new GraphId(value2);

            Assert.AreEqual(isSuccess, id1 != id2);
            Assert.AreEqual(isSuccess, id2 != id1);
        }

        [TestCase("123", "123", false)]
        [TestCase("123", 123, true)]
        [TestCase("123", null, true)]
        public void NotEqualsOperator_AgainstObject(string value1, object value2, bool isSuccess)
        {
            var id1 = new GraphId(value1);

            Assert.AreEqual(isSuccess, id1 != value2);
            Assert.AreEqual(isSuccess, value2 != id1);
        }

        [Test]
        public void NotEqualsOperatorAgainstSelf_IsFalse()
        {
            var id = new GraphId("123");

#pragma warning disable CS1718 // Comparison made to same variable
            Assert.IsFalse(id != id);
#pragma warning restore CS1718 // Comparison made to same variable
        }
    }
}