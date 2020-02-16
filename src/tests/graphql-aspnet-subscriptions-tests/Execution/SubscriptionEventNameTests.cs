// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.Execution
{
    using System;
    using GraphQL.AspNet.Execution.Subscriptions;
    using GraphQL.AspNet.Schemas;
    using NUnit.Framework;

    [TestFixture]
    public class SubscriptionEventNameTests
    {
        [Test]
        public void GeneralPropertyCheck()
        {
            var id = new SubscriptionEventName("schema", "abc");
            Assert.AreEqual("schema:abc", id.ToString());
            Assert.AreEqual("schema:abc".GetHashCode(), id.GetHashCode());
        }

        [Test]
        public void GeneralPropertyCheck_FromType()
        {
            var id = new SubscriptionEventName(typeof(GraphSchema), "abc");
            Assert.AreEqual($"{typeof(GraphSchema).FullName}:abc", id.ToString());
            Assert.AreEqual($"{typeof(GraphSchema).FullName}:abc".GetHashCode(), id.GetHashCode());
        }

        [TestCase("schema:abc", "schema", "abc")]
        [TestCase("schema:", null)]
        [TestCase(":abc", null)]
        [TestCase("schema", null)]
        [TestCase("schema:  ", null)]
        [TestCase("  :abc", null)]
        public void UnPack(string packedValue, string schema, string value = null)
        {
            var result = SubscriptionEventName.UnPack(packedValue);

            if (schema == null)
            {
                Assert.IsNull(result);
            }
            else
            {
                Assert.AreEqual(new SubscriptionEventName(schema, value), result);
            }
        }

        [Test]
        public void EqualsMethodAgainstNonString_DoesNotMatch()
        {
            var id = new SubscriptionEventName("schema", "abc");

            var idValue = (object)123;
            Assert.IsFalse(id.Equals(idValue));
        }

        [Test]
        public void EqualsMethodAgainstNull_DoesNotMatch()
        {
            var id = new SubscriptionEventName("schema", "abc");
            Assert.IsFalse(id.Equals(null as object));
        }

        [Test]
        public void EqualsMethodAgainstNullAsEvent_DoesNotMatch()
        {
            var id = new SubscriptionEventName("schema", "abc");
            Assert.IsFalse(id.Equals(null as SubscriptionEventName));
        }

        [Test]
        public void EqualsMethodAgainstSelf_Matches()
        {
            var id = new SubscriptionEventName("schema", "abc");
            Assert.IsTrue(id.Equals(id));
        }

        [Test]
        public void EqualsMethodAgainstEvent_Matches()
        {
            var id = new SubscriptionEventName("schema", "abc");
            var id2 = new SubscriptionEventName("schema", "abc");

            Assert.IsTrue(id.Equals(id2));
        }

        [Test]
        public void EqualsMethodAgainstSelfAsObject_Matches()
        {
            var id = new SubscriptionEventName("schema", "abc");
            Assert.IsTrue(id.Equals((object)id));
        }

        [Test]
        public void EqualsMethodAgainstOtherAsObject_Matches()
        {
            var id = new SubscriptionEventName("schema", "abc");
            var secondId = new SubscriptionEventName("schema", "abc");

            Assert.IsTrue(id.Equals((object)secondId));
        }

        [Test]
        public void EqualsMethodAgainstStringObject_Matches()
        {
            var id = new SubscriptionEventName("schema", "abc");

            var obj = (object)"schema:abc";
            Assert.IsTrue(id.Equals(obj));
        }

        [TestCase("schema", "123", "schema", "123", true)]
        [TestCase(null, null, null, null, false)]
        [TestCase("", "", "", "", false)]
        [TestCase("schema", "123", null, null, false)]
        [TestCase(null, null, "schema", "123", false)]
        [TestCase("", "", null, null, false)]
        [TestCase(null, null, "", "", false)]
        [TestCase("schema", "123", "schema", "345", false)]
        public void EqualsOperator(string schema1, string value1, string schema2, string value2, bool isSuccess)
        {
            var id1 = new SubscriptionEventName(schema1, value1);
            var id2 = new SubscriptionEventName(schema2, value2);

            Assert.AreEqual(isSuccess, id1 == id2);
            Assert.AreEqual(isSuccess, id2 == id1);
        }

        [Test]
        public void EqualsOperatorAgainstSelf_IsTrue()
        {
            var id = new SubscriptionEventName("schema", "123");

            // ReSharper disable once EqualExpressionComparison
#pragma warning disable CS1718 // Comparison made to same variable
            Assert.IsTrue(id == id);
#pragma warning restore CS1718 // Comparison made to same variable
        }

        [TestCase("schema", "123", "schema:123", true)]
        [TestCase("schema", "123", 123, false)]
        [TestCase("schema", "123", null, false)]
        public void EqualsOperator_AgainstObject(string schema1, string value1, object value2, bool isSuccess)
        {
            var id1 = new SubscriptionEventName(schema1, value1);

            Assert.AreEqual(isSuccess, id1 == value2);
            Assert.AreEqual(isSuccess, value2 == id1);
        }

        [TestCase("schema", "123", "schema", "123", false)]
        [TestCase(null, null, null, null, true)]
        [TestCase("", "", "", "", true)]
        [TestCase("schema", "123", null, null, true)]
        [TestCase(null, null, "schema", "123", true)]
        [TestCase("", "", null, null, true)]
        [TestCase(null, null, "", "", true)]
        [TestCase("schema", "123", "schema", "345", true)]
        public void NotEqualsOperator(string schema1, string value1, string schema2, string value2, bool isSuccess)
        {
            var id1 = new SubscriptionEventName(schema1, value1);
            var id2 = new SubscriptionEventName(schema2, value2);

            Assert.AreEqual(isSuccess, id1 != id2);
            Assert.AreEqual(isSuccess, id2 != id1);
        }

        [TestCase("schema", "123", "schema:123", false)]
        [TestCase("schema", "123", 123, true)]
        [TestCase("schema", "123", null, true)]
        public void NotEqualsOperator_AgainstObject(string schema1, string value1, object value2, bool isSuccess)
        {
            var id1 = new SubscriptionEventName(schema1, value1);

            Assert.AreEqual(isSuccess, id1 != value2);
            Assert.AreEqual(isSuccess, value2 != id1);
        }

        [Test]
        public void NotEqualsOperatorAgainstSelf_IsFalse()
        {
            var id = new SubscriptionEventName("schema", "123");

#pragma warning disable CS1718 // Comparison made to same variable
            Assert.IsFalse(id != id);
#pragma warning restore CS1718 // Comparison made to same variable
        }
    }
}