// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

#pragma warning disable SA1116 // Split parameters should start on line after declaration
namespace GraphQL.AspNet.Tests.Schemas
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Engine;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Schemas.TypeSystem.Scalars;
    using GraphQL.AspNet.Tests.Common.CommonHelpers;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Schemas.QueryLanguageTestData;
    using NUnit.Framework;

    [TestFixture]
    public class SchemaLanguageGeneratorTests
    {
        private List<Type> _unUsedScalarTypes;
        private static List<(object TestObject, string ExpectedResult)> _testValues;
        private GraphSchema _schema;

        private static void SetupTestData()
        {
            _testValues = new List<(object, string)>();

            _testValues.Add((null, "null"));

            _testValues.Add((Happiness.Happy, "HAPPY"));
            _testValues.Add((Happiness.Sad, "SAD"));
            _testValues.Add(((Happiness)45, "null"));

            _testValues.Add(((sbyte)5, "5"));
            _testValues.Add(((sbyte)-5, "-5"));
            _testValues.Add(((sbyte)0, "0"));

            _testValues.Add(((byte)5, "5"));
            _testValues.Add(((byte)0, "0"));
            _testValues.Add(((byte)138, "138"));

            _testValues.Add((5, "5"));
            _testValues.Add((-5, "-5"));
            _testValues.Add((5000000, "5000000"));
            _testValues.Add((-5000000, "-5000000"));
            _testValues.Add((0, "0"));

            _testValues.Add((5U, "5"));
            _testValues.Add((5000000U, "5000000"));
            _testValues.Add((0U, "0"));

            _testValues.Add(((short)5, "5"));
            _testValues.Add(((short)-5, "-5"));
            _testValues.Add(((short)5000, "5000"));
            _testValues.Add(((short)-5000, "-5000"));
            _testValues.Add(((short)0, "0"));

            _testValues.Add(((ushort)5, "5"));
            _testValues.Add(((ushort)5000, "5000"));
            _testValues.Add(((ushort)0, "0"));

            _testValues.Add((5L, "5"));
            _testValues.Add((-5L, "-5"));
            _testValues.Add((5000000L, "5000000"));
            _testValues.Add((-5000000L, "-5000000"));
            _testValues.Add((0L, "0"));

            _testValues.Add((5UL, "5"));
            _testValues.Add((5000000UL, "5000000"));
            _testValues.Add((0UL, "0"));
            _testValues.Add((5f, "5"));
            _testValues.Add((-5f, "-5"));
            _testValues.Add((5000000f, "5000000"));
            _testValues.Add((-5000000f, "-5000000"));
            _testValues.Add((500.23f, "500.23"));
            _testValues.Add((-500.23f, "-500.23"));
            _testValues.Add((0f, "0"));

            _testValues.Add((5m, "5"));
            _testValues.Add((-5m, "-5"));
            _testValues.Add((5000000m, "5000000"));
            _testValues.Add((-5000000m, "-5000000"));
            _testValues.Add((500.23m, "500.23"));
            _testValues.Add((-500.23m, "-500.23"));
            _testValues.Add((0m, "0"));

            _testValues.Add((5d, "5"));
            _testValues.Add((-5d, "-5"));
            _testValues.Add((5000000d, "5000000"));
            _testValues.Add((-5000000d, "-5000000"));
            _testValues.Add((500.23d, "500.23"));
            _testValues.Add((-500.23d, "-500.23"));
            _testValues.Add((0d, "0"));

            _testValues.Add(((GraphId)"123", "\"123\""));
            _testValues.Add(((GraphId)"abc", "\"abc\""));
            _testValues.Add(((GraphId)"씨엘씨", "\"\\uc528\\uc5d8\\uc528\""));
            _testValues.Add(((GraphId)string.Empty, "\"\""));

            _testValues.Add((true, "true"));
            _testValues.Add((false, "false"));

            _testValues.Add((new DateTime(2022, 8, 11, 12, 31, 22, 0, DateTimeKind.Utc), "\"2022-08-11T12:31:22.000+00:00\""));
            _testValues.Add((new DateTimeOffset(2022, 8, 11, 12, 31, 22, 0, TimeSpan.Zero), "\"2022-08-11T12:31:22.000+00:00\""));

            _testValues.Add((new Uri("http://somesite.com", UriKind.Absolute), "\"http://somesite.com/\""));
            _testValues.Add((new Uri("/relativeUri/path1", UriKind.Relative), "\"/relativeUri/path1\""));

            _testValues.Add(("test string", "\"test string\""));
            _testValues.Add(("   white\tspace   ", "\"   white\tspace   \""));
            _testValues.Add(("test string", "\"test string\""));
            _testValues.Add(("있지", "\"\\uc788\\uc9c0\""));
            _testValues.Add((string.Empty, "\"\""));

            _testValues.Add((Guid.Parse("80332679-35FB-4FB5-8DF6-A7A8F91E4E05"), "\"80332679-35fb-4fb5-8df6-a7a8f91e4e05\""));

#if NET6_0_OR_GREATER
            _testValues.Add((new DateOnly(2022, 8, 11), "\"2022-08-11\""));
            _testValues.Add((new TimeOnly(17, 35, 18, 50), "\"17:35:18.050\""));
#endif
            _testValues.Add((new TwoPropertyObject(), "{ property1: null property2: 0 }"));
            _testValues.Add((new TwoPropertyObject() { Property1 = "str", Property2 = 5 }, "{ property1: \"str\" property2: 5 }"));
            _testValues.Add((new TwoPropertyObject() { Property1 = "null", Property2 = 99 }, "{ property1: \"null\" property2: 99 }"));
            _testValues.Add((new TwoPropertyObject() { Property1 = "st\"ring", Property2 = -5 }, "{ property1: \"st\\u0022ring\" property2: -5 }"));

            var parent = new Person("StandardPerson") { Name = "Bob", Child = new Person("standardChild") { Name = "Jane" } };
            _testValues.Add((parent, "{ name: \"Bob\" child: { name: \"Jane\" child: null } }"));

            var unicodePerson = new Person("UnicodePerson") { Name = "최예나" };
            _testValues.Add((unicodePerson, "{ name: \"\\ucd5c\\uc608\\ub098\" child: null }"));

            var moodyPerson = new MoodyPerson("moodyPerson") { Name = "Joe", HappinessLevel = Happiness.Sad };
            _testValues.Add((moodyPerson, "{ name: \"Joe\" happinessLevel: SAD }"));

            // ensure that same level repeat references are fine
            var grandParent = new GrandParent("grandparent") { Name = "grando" };
            var child = new Person("child1") { Name = "child1" };
            grandParent.Child1 = child;
            grandParent.Child2 = child;
            _testValues.Add((
                grandParent,
                "{ name: \"grando\" child1: { name: \"child1\" child: null } child2: { name: \"child1\" child: null } }"));

            // additional input field tests
            var slug = new TestSlug()
            {
                DateTime = new DateTime(2022, 08, 12, 1, 2, 3, 4, DateTimeKind.Utc),
                DateTimeOffset = new DateTimeOffset(2022, 08, 12, 15, 26, 37, 458, TimeSpan.Zero),
                Int = 33,
                Double = 123.456789,
                Uri = new Uri("https://github.com/graphql-aspnet"),
                GraphId = (GraphId)"에스파-안무",
                String = null,
            };
            _testValues.Add((slug, "{ dateTime: \"2022-08-12T01:02:03.004+00:00\" dateTimeOffset: \"2022-08-12T15:26:37.458+00:00\" int: 33 double: 123.456789 uri: \"https://github.com/graphql-aspnet\" graphId: \"\\uc5d0\\uc2a4\\ud30c-\\uc548\\ubb34\" string: null }"));

            // deep nested object
            var nestableItem = new NestableObject(0,
                new NestableObject(1,
                    new NestableObject(2,
                        new NestableObject(3,
                            new NestableObject(4,
                                new NestableObject(5,
                                    new NestableObject(6,
                                        new NestableObject(7,
                                            new NestableObject(8)))))))));

            _testValues.Add((nestableItem, "{ value: 0 next: { value: 1 next: { value: 2 next: { value: 3 next: { value: 4 next: { value: 5 next: { value: 6 next: { value: 7 next: { value: 8 next: null } } } } } } } } }"));

            // scalar list
            _testValues.Add((new List<int>() { 1, 2, 3 }, "[1, 2, 3]"));
            _testValues.Add((new List<int>(), "[]"));
            _testValues.Add((new List<string>() { "a", "b", "c" }, "[\"a\", \"b\", \"c\"]"));
            _testValues.Add((new List<string>() { "a", "b", null }, "[\"a\", \"b\", null]"));
            _testValues.Add((new List<int?>() { 1, null, 3 }, "[1, null, 3]"));
            _testValues.Add((new List<int?>() { 1, null, null, 4 }, "[1, null, null, 4]"));
            _testValues.Add((new List<int?>() { null, null, null, null }, "[null, null, null, null]"));
            _testValues.Add((new List<int?>(), "[]"));
            _testValues.Add((new List<IEnumerable<int>>() { new List<int>() { 1, 2, 3 }, new List<int>() { 4, 5, 6 } }, "[[1, 2, 3], [4, 5, 6]]"));
            _testValues.Add((new List<IEnumerable<int[]>>() { new List<int[]>() { new int[] { 1 }, new int[] { 2, 3 }, new int[] { 4 } }, new List<int[]>() { new int[] { 5 }, new int[] { 6 }, new int[] { 7, 8 } } }, "[[[1], [2, 3], [4]], [[5], [6], [7, 8]]]"));
            _testValues.Add((new List<IEnumerable<int[]>>(), "[]"));
            _testValues.Add((new List<IEnumerable<int[]>>() { new List<int[]>() { } }, "[[]]"));
            _testValues.Add((new List<IEnumerable<int[]>>() { new List<int[]>() { new int[0] } }, "[[[]]]"));
            _testValues.Add((new List<IEnumerable<int[]>>() { new List<int[]>() { new int[] { 99 } } }, "[[[99]]]"));

            // enum list
            _testValues.Add((new List<Happiness>() { Happiness.Sad, Happiness.Happy, Happiness.Sad }, "[SAD, HAPPY, SAD]"));
            _testValues.Add((new List<Happiness>(), "[]"));
            _testValues.Add((new List<Happiness?>() { Happiness.Sad, null, Happiness.Happy }, "[SAD, null, HAPPY]"));
            _testValues.Add((new List<Happiness>() { (Happiness)1 }, "[SAD]"));

            // input_object list
            _testValues.Add(
                (new List<TwoPropertyObject>() { new TwoPropertyObject() { Property1 = "str", Property2 = 5 }, new TwoPropertyObject() { Property1 = "str1", Property2 = 8 } },
                    "[{ property1: \"str\" property2: 5 }, { property1: \"str1\" property2: 8 }]"));
            _testValues.Add(
                (new List<TwoPropertyObject>() { new TwoPropertyObject() { Property1 = "str", Property2 = 5 }, null },
                    "[{ property1: \"str\" property2: 5 }, null]"));
            _testValues.Add((new List<TwoPropertyObject>(), "[]"));
            _testValues.Add((new List<TwoPropertyObject>() { null, null, null }, "[null, null, null]"));
        }

        static SchemaLanguageGeneratorTests()
        {
            SetupTestData();
        }

        public SchemaLanguageGeneratorTests()
        {
            var typesUsed = _testValues
                .Where(x => x.TestObject != null)
                .Select(x => x.TestObject.GetType())
                .ToHashSet();

            // add all test types to the schema
            var serverBuilder = new TestServerBuilder();
            serverBuilder.AddType(typeof(Happiness));
            serverBuilder.AddType(typeof(TwoPropertyObject), TypeKind.INPUT_OBJECT);
            serverBuilder.AddType(typeof(Person), TypeKind.INPUT_OBJECT);
            serverBuilder.AddType(typeof(MoodyPerson), TypeKind.INPUT_OBJECT);
            serverBuilder.AddType(typeof(GrandParent), TypeKind.INPUT_OBJECT);
            serverBuilder.AddType(typeof(TestSlug), TypeKind.INPUT_OBJECT);
            serverBuilder.AddType(typeof(NestableObject), TypeKind.INPUT_OBJECT);

            // ensure all scalars represented
            _unUsedScalarTypes = new List<Type>();
            foreach (var type in GlobalTypes.ScalarConcreteTypes)
            {
                if (Validation.IsNullableOfT(type))
                    continue;

                if (!typesUsed.Contains(type))
                    _unUsedScalarTypes.Add(type);
                else
                    serverBuilder.AddType(type);
            }

            // create the schema
            var server = serverBuilder.Build();
            _schema = server.Schema;
        }

        [TestCaseSource(nameof(_testValues))]
        public void SerializeObject((object InputObject, string ExpectedValue) testData)
        {
            var generator = new SchemaLanguageGenerator(_schema);
            var result = generator.SerializeObject(testData.InputObject);
            Assert.AreEqual(testData.ExpectedValue, result);
        }

        [Test]
        public void NoUnTestedScalars()
        {
            Assert.AreEqual(
                0,
                _unUsedScalarTypes.Count,
                "One or more built in scalars is not covered in " +
                $"the {nameof(SerializeObject)} unit test set.");
        }

        [Test]
        public void CircularReference_ThrowsException()
        {
            var loopedPerson = new Person("LoopedPerson") { Name = "Bob", Child = null };
            loopedPerson.Child = loopedPerson;

            Assert.Throws<InvalidOperationException>(() =>
            {
                var generator = new SchemaLanguageGenerator(_schema);
                generator.SerializeObject(loopedPerson);
            });
        }

        [Test]
        public void InvalidObjectType_ThrowsException()
        {
            var dic = new Dictionary<string, string>();

            Assert.Throws<InvalidOperationException>(() =>
            {
                var generator = new SchemaLanguageGenerator(_schema);
                generator.SerializeObject(dic);
            });
        }

        [Test]
        public void UnknownObjectType_ThrowsException()
        {
            // V3 is not registered to the schema
            var obj = new TwoPropertyObjectV3()
            {
                Property1 = "bob",
                Property2 = DateTime.UtcNow,
            };

            Assert.Throws<InvalidOperationException>(() =>
            {
                var generator = new SchemaLanguageGenerator(_schema);
                generator.SerializeObject(obj);
            });
        }

        [Test]
        public void UnknownEnumValue_ThrowsException()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                // melancholy is declared on the enum with [GraphSkip]
                // the schema won't know of the value, so it can't be serialzied
                var generator = new SchemaLanguageGenerator(_schema);
                generator.SerializeObject(Happiness.Melancholy);
            });
        }

        [Test]
        public void UnknownInListOfEnumValue_ThrowsException()
        {
            var list = new List<Happiness>() { Happiness.Happy, Happiness.Sad, Happiness.Melancholy };
            Assert.Throws<InvalidOperationException>(() =>
            {
                // melancholy is declared on the enum with [GraphSkip]
                // the schema won't know of the value, so it can't be serialzied
                var generator = new SchemaLanguageGenerator(_schema);
                generator.SerializeObject(list);
            });
        }
    }
}