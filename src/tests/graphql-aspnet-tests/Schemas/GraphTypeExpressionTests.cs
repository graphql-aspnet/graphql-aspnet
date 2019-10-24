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
    using NUnit.Framework;
    using GTW = GraphQL.AspNet.Schemas.TypeSystem.MetaGraphTypes;

    [TestFixture]
    public class GraphTypeExpressionTests
    {
        public static readonly List<object[]> TypeExpressionObjectTests;

        static GraphTypeExpressionTests()
        {
            TypeExpressionObjectTests = new List<object[]>();
            TypeExpressionObjectTests.Add(new object[] { 5, new GTW[0], true, });
            TypeExpressionObjectTests.Add(new object[] { (int?)5, new GTW[0], true, });
            TypeExpressionObjectTests.Add(new object[] { null, new GTW[0], true, });
            TypeExpressionObjectTests.Add(new object[] { default(int?), new GTW[0], true, });
            TypeExpressionObjectTests.Add(new object[] { default(int?), new[] { GTW.IsNotNull }, false, });
            TypeExpressionObjectTests.Add(new object[] { "bobSmith", new[] { GTW.IsList }, false, });
            TypeExpressionObjectTests.Add(new object[] { "bobSmith", new[] { GTW.IsNotNull }, true, });
            TypeExpressionObjectTests.Add(new object[] { "bobSmith", new GTW[0], true, });
            TypeExpressionObjectTests.Add(new object[] { null, new[] { GTW.IsList }, true, });
            TypeExpressionObjectTests.Add(new object[] { new List<int>(), new[] { GTW.IsList }, true, });
            TypeExpressionObjectTests.Add(new object[]
            {
                new List<int>
                {
                    5,
                    5,
                },
                new[] { GTW.IsList, GTW.IsNotNull }, true,
            });
            TypeExpressionObjectTests.Add(new object[]
            {
                new List<int?>
                {
                    5,
                    null,
                },
                new[] { GTW.IsList, GTW.IsNotNull }, false,
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
                new[] { GTW.IsList, GTW.IsList, GTW.IsNotNull }, true,
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
                new[] { GTW.IsList, GTW.IsList, GTW.IsNotNull }, false,
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
                new[] { GTW.IsList, GTW.IsList, }, true,
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
                new[] { GTW.IsList, GTW.IsNotNull, GTW.IsList, }, true,
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
                new[] { GTW.IsList, GTW.IsNotNull, GTW.IsList, }, false,
            });
        }

        [TestCaseSource(nameof(TypeExpressionObjectTests))]
        public void Matches_TestObject(object objectToTest, IEnumerable<GTW> wrappers, bool expectedResult)
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
        [TestCase("a", true, new GTW[0], "a")]
        [TestCase("[SomeType!", false, null, null)]
        [TestCase("Some!Type", true, new GTW[0], "Some!Type")]
        [TestCase("SomeType!!", false, null, null)]
        [TestCase("[[SomeType!]!", false, null, null)]
        [TestCase("[[SomeType!]]!", true, new[] { GTW.IsNotNull, GTW.IsList, GTW.IsList, GTW.IsNotNull }, "SomeType")]
        [TestCase("[SomeType!]!", true, new[] { GTW.IsNotNull, GTW.IsList, GTW.IsNotNull }, "SomeType")]
        [TestCase("[SomeType]!", true, new[] { GTW.IsNotNull, GTW.IsList }, "SomeType")]
        [TestCase("[SomeType!]", true, new[] { GTW.IsList, GTW.IsNotNull }, "SomeType")]
        [TestCase("[SomeType]", true, new[] { GTW.IsList }, "SomeType")]
        [TestCase("SomeType", true, new GTW[0], "SomeType")]
        public void ParseDeclaration(
            string declarationText,
            bool isValid,
            GTW[] expectedModifiers,
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
    }
}