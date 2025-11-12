// *************************************************************
//  project:  graphql-aspnet
//  --
//  repo: https://github.com/graphql-aspnet
//  docs: https://graphql-aspnet.github.io
//  --
//  License:  MIT
//  *************************************************************

namespace GraphQL.AspNet.Tests.Execution
{
    using System;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using NUnit.Framework;

    [TestFixture]
    public class GraphInputUnionTests
    {
        public class TestUnion<TPropType> : GraphInputUnion
        {
            public TPropType Prop1 { get; set; }
        }

        [TestCase(null, "default string", "default string")]
        [TestCase("parsed string", "default string", "parsed string")]
        [TestCase("parsed string", null, "parsed string")]
        [TestCase(null, null, null)]
        public void ValueOrDefault_ShouldReturnExpectedValues_WhenTypesAreCompatible_String(
            string supplied,
            string fallback,
            string expected)
        {
            var obj = new TestUnion<string>
            {
                Prop1 = supplied,
            };

            var prop1Result = obj.ValueOrDefault(x => x.Prop1, fallback);
            Assert.That(prop1Result, Is.EqualTo(expected));
        }

        [TestCase(null, 15, 15)]
        [TestCase(30, 15, 30)]
        public void ValueOrDefault_ShouldReturnExpectedValues_WhenTypesAreCompatible_Int(
            int? supplied,
            int fallback,
            int expected)
        {
            var obj = new TestUnion<int?>
            {
                Prop1 = supplied,
            };

            var prop1Result = obj.ValueOrDefault(x => x.Prop1, fallback);
            Assert.That(prop1Result, Is.EqualTo(expected));
        }

        [Test]
        public void ValueOrDefault_ShouldThrowException_WhenIncompatiableNumericTypes()
        {
            var obj = new TestUnion<int>
            {
                Prop1 = 15,
            };

            Assert.Throws<InvalidOperationException>(() => obj.ValueOrDefault(x => x.Prop1, 41L));
        }

        [Test]
        public void ValueOrDefault_ShouldThrowException_WhenValueTypeAgainstClassType()
        {
            var obj = new TestUnion<TwoPropertyObject>
            {
                Prop1 = new TwoPropertyObject
                {
                    Property1 = "prop value",
                },
            };

            // object vs long
            Assert.Throws<InvalidOperationException>(() => obj.ValueOrDefault(x => x.Prop1, 41L));
        }
    }
}