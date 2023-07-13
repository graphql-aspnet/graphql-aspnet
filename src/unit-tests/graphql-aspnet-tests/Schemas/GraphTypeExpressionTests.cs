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
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Tests.Common.CommonHelpers;
    using NUnit.Framework;
    using MGT = GraphQL.AspNet.Schemas.TypeSystem.MetaGraphTypes;

    [TestFixture]
    public class GraphTypeExpressionTests
    {
        public static readonly List<object[]> TypeExpressionObjectTests;

        static GraphTypeExpressionTests()
        {
            TypeExpressionObjectTests = new List<object[]>();
            TypeExpressionObjectTests.Add(new object[] { 5, new MGT[0], true, });
            TypeExpressionObjectTests.Add(new object[] { (int?)5, new MGT[0], true, });
            TypeExpressionObjectTests.Add(new object[] { null, new MGT[0], true, });
            TypeExpressionObjectTests.Add(new object[] { default(int?), new MGT[0], true, });
            TypeExpressionObjectTests.Add(new object[] { default(int?), new[] { MGT.IsNotNull }, false, });
            TypeExpressionObjectTests.Add(new object[] { "bobSmith", new[] { MGT.IsList }, false, });
            TypeExpressionObjectTests.Add(new object[] { "bobSmith", new[] { MGT.IsNotNull }, true, });
            TypeExpressionObjectTests.Add(new object[] { "bobSmith", new MGT[0], true, });
            TypeExpressionObjectTests.Add(new object[] { null, new[] { MGT.IsList }, true, });
            TypeExpressionObjectTests.Add(new object[] { new List<int>(), new[] { MGT.IsList }, true, });
            TypeExpressionObjectTests.Add(new object[]
            {
                new List<int>
                {
                    5,
                    5,
                },
                new[] { MGT.IsList, MGT.IsNotNull }, true,
            });
            TypeExpressionObjectTests.Add(new object[]
            {
                new List<int?>
                {
                    5,
                    null,
                },
                new[] { MGT.IsList, MGT.IsNotNull }, false,
            });
            TypeExpressionObjectTests.Add(new object[]
            {
                new List<List<int>>
                {
                    new List<int>
                    {
                        5,
                        5,
                    },
                },
                new[] { MGT.IsList, MGT.IsList, MGT.IsNotNull }, true,
            });
            TypeExpressionObjectTests.Add(new object[]
            {
                new List<List<int?>>
                {
                    new List<int?>
                    {
                        5,
                        null,
                    },
                },
                new[] { MGT.IsList, MGT.IsList, MGT.IsNotNull }, false,
            });
            TypeExpressionObjectTests.Add(new object[]
            {
                new List<List<int?>>
                {
                    new List<int?>
                    {
                        5,
                        null,
                    },
                },
                new[] { MGT.IsList, MGT.IsList, }, true,
            });
            TypeExpressionObjectTests.Add(new object[]
            {
                new List<List<int?>>
                {
                    new List<int?>
                    {
                        5,
                        null,
                    },
                },
                new[] { MGT.IsList, MGT.IsNotNull, MGT.IsList, }, true,
            });
            TypeExpressionObjectTests.Add(new object[]
            {
                new List<List<int>>
                {
                    new List<int>
                    {
                        5,
                        8,
                    },
                    null,
                },
                new[] { MGT.IsList, MGT.IsNotNull, MGT.IsList, }, false,
            });
        }

        [TestCaseSource(nameof(TypeExpressionObjectTests))]
        public void Matches_TestObject(object objectToTest, IEnumerable<MGT> wrappers, bool expectedResult)
        {
            var typeExpression = new GraphTypeExpression("SomeType", wrappers?.ToArray());

            var matches = typeExpression.Matches(objectToTest);

            Assert.AreEqual(expectedResult, matches);
        }

        [TestCase("[]!", false, null, null)]
        [TestCase("[]", false, null, null)]
        [TestCase("[", false, null, null)]
        [TestCase("]", false, null, null)]
        [TestCase("!", false, null, null)]
        [TestCase("", false, null, null)]
        [TestCase("a", true, new MGT[0], "a")]
        [TestCase("[SomeType!", false, null, null)]
        [TestCase("Some!Type", true, new MGT[0], "Some!Type")]
        [TestCase("SomeType!!", false, null, null)]
        [TestCase("[[SomeType!]!", false, null, null)]
        [TestCase("[[SomeType!]]!", true, new[] { MGT.IsNotNull, MGT.IsList, MGT.IsList, MGT.IsNotNull }, "SomeType")]
        [TestCase("[SomeType!]!", true, new[] { MGT.IsNotNull, MGT.IsList, MGT.IsNotNull }, "SomeType")]
        [TestCase("[SomeType]!", true, new[] { MGT.IsNotNull, MGT.IsList }, "SomeType")]
        [TestCase("[SomeType!]", true, new[] { MGT.IsList, MGT.IsNotNull }, "SomeType")]
        [TestCase("[SomeType]", true, new[] { MGT.IsList }, "SomeType")]
        [TestCase("SomeType", true, new MGT[0], "SomeType")]
        public void ParseDeclaration(
            string declarationText,
            bool isValid,
            MGT[] expectedModifiers,
            string expectedTypeName)
        {
            var declaration = GraphTypeExpression.FromDeclaration(declarationText.AsSpan());
            Assert.AreEqual(isValid, declaration.IsValid);

            if (isValid)
            {
                Assert.AreEqual(expectedTypeName, declaration.TypeName);
                Assert.AreEqual(expectedModifiers.Length, declaration.Wrappers.Length);
                for (var i = 0; i < expectedModifiers.Length; i++)
                {
                    Assert.AreEqual(expectedModifiers[i], declaration.Wrappers[i]);
                }

                Assert.AreEqual(declarationText, declaration.ToString());
            }
        }

        [TestCase(null, "String", false)]
        [TestCase("String", null, false)]
        [TestCase(null, null, false)]
        [TestCase("String", "String", true)]
        [TestCase("String", "String", true)]
        [TestCase("[String]", "[String]", true)]
        [TestCase("[String!]", "[String!]", true)]
        [TestCase("[String]!", "[String]!", true)]
        [TestCase("[String!]!", "[String!]!", true)]
        [TestCase("String", "Int", false)]
        [TestCase("String", "string", false)]
        [TestCase("String", "String!", true)]
        [TestCase("String", "[String]", false)]
        [TestCase("[String]", "String", false)]
        [TestCase("[Int]", "[Int]!", true)]
        [TestCase("[Int!]", "[Int]", false)]
        [TestCase("[Int]", "[Int!]", true)]
        [TestCase("[Int]", "[[Int]]", false)]
        [TestCase("[[[Int]!]]", "[[[Int!]!]!]!", true)]
        [TestCase("[[[Int]!]]!", "[[[Int!]!]!]!", true)]
        [TestCase("[[[Int!]]!]", "[[[Int!]!]!]!", true)]
        [TestCase("[[[String!]]!]", "[[[Float!]!]!]!", false)]
        public void AreCompatiable(string targetExpression, string suppliedExpression, bool shouldBeCompatiable)
        {
            var target = targetExpression == null ? null : GraphTypeExpression.FromDeclaration(targetExpression);
            var supplied = suppliedExpression == null ? null : GraphTypeExpression.FromDeclaration(suppliedExpression);

            var result = GraphTypeExpression.AreTypesCompatiable(target, supplied);

            Assert.AreEqual(shouldBeCompatiable, result);
        }

        [TestCase(typeof(int), "int!", null)]
        [TestCase(typeof(IEnumerable<int>), "[int!]", null)]
        [TestCase(typeof(int[]), "[int!]", null)]
        [TestCase(typeof(IEnumerable<TwoPropertyObject>), "[TwoPropertyObject]", null)]
        [TestCase(typeof(TwoPropertyObject[]), "[TwoPropertyObject]", null)]
        [TestCase(typeof(int[][]), "[[int!]]", null)]
        [TestCase(typeof(string[]), "[string]", null)]
        [TestCase(typeof(KeyValuePair<string, int>), "KeyValuePair_string_int_!", null)]
        [TestCase(typeof(KeyValuePair<string[], int[][]>), "KeyValuePair_string___int_____!", null)]
        [TestCase(typeof(KeyValuePair<string, int[]>), "KeyValuePair_string_int___!", null)]
        [TestCase(typeof(KeyValuePair<string, int[]>[]), "[KeyValuePair_string_int___!]", null)]
        [TestCase(typeof(KeyValuePair<string[][], int[][][]>[]), "[KeyValuePair_string_____int_______!]", null)]
        [TestCase(typeof(KeyValuePair<string[][], int[][][]>[][]), "[[KeyValuePair_string_____int_______!]]", null)]
        [TestCase(typeof(KeyValuePair<string, int>[]), "[KeyValuePair_string_int_!]", null)]
        [TestCase(typeof(List<KeyValuePair<string, int>>), "[KeyValuePair_string_int_!]", null)]
        [TestCase(typeof(IEnumerable<IEnumerable<int>>), "[[int!]]", null)]
        [TestCase(typeof(IEnumerable<IEnumerable<int>>), "int!", new MGT[] { MGT.IsNotNull })]
        public void GenerateTypeExpression(
            Type type,
            string expectedExpression,
            MGT[] wrappers)
        {
            var typeExpression = GraphTypeExpression.FromType(type, wrappers);
            Assert.AreEqual(expectedExpression, typeExpression.ToString());
        }
    }
}