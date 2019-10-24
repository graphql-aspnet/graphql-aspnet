// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Internal
{
    using System;
    using System.Collections.Generic;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Internal;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Tests.CommonHelpers;
    using Moq;
    using NUnit.Framework;
    using GTW = GraphQL.AspNet.Schemas.TypeSystem.MetaGraphTypes;

    [TestFixture]
    public class GraphValidationTests
    {
        [TestCase(typeof(IEnumerable<int>), true)]
        [TestCase(typeof(ICollection<int>), true)]
        [TestCase(typeof(IList<int>), true)]
        [TestCase(typeof(List<int>), true)]
        [TestCase(typeof(List<string>), true)]
        [TestCase(typeof(List<DateTime>), true)]
        [TestCase(typeof(List<TwoPropertyObject>), true)]
        [TestCase(typeof(string), false)]
        [TestCase(typeof(int), false)]
        [TestCase(typeof(DateTime), false)]
        [TestCase(typeof(Dictionary<string, int>), false)]
        [TestCase(typeof(IDictionary<string, int>), false)]
        [TestCase(typeof(IReadOnlyDictionary<string, int>), false)]
        public void IsValidListType(Type inputType, bool isValidListType)
        {
            var result = GraphValidation.IsValidListType(inputType);
            Assert.AreEqual(isValidListType, result);
        }

        [TestCase(typeof(IEnumerable<int>), true)]
        [TestCase(typeof(ICollection<int>), true)]
        [TestCase(typeof(IList<int>), true)]
        [TestCase(typeof(List<int>), true)]
        [TestCase(typeof(List<string>), true)]
        [TestCase(typeof(List<DateTime>), true)]
        [TestCase(typeof(List<TwoPropertyObject>), true)]
        [TestCase(typeof(string), true)]
        [TestCase(typeof(int), true)]
        [TestCase(typeof(DateTime), true)]
        [TestCase(typeof(Dictionary<string, int>), false)]
        [TestCase(typeof(IDictionary<string, int>), false)]
        [TestCase(typeof(IReadOnlyDictionary<string, int>), false)]
        public void IsValidGraphType(Type inputType, bool isValidType)
        {
            var result = GraphValidation.IsValidGraphType(inputType);
            Assert.AreEqual(isValidType, result);
            if (!result)
            {
                Assert.Throws<GraphTypeDeclarationException>(() =>
                {
                    GraphValidation.IsValidGraphType(inputType, true);
                });
            }
        }

        [TestCase(typeof(int), "int!", false, null)]
        [TestCase(typeof(int), "int", true, null)]
        [TestCase(typeof(IEnumerable<int>), "[int!]", false, null)]
        [TestCase(typeof(IEnumerable<IEnumerable<int>>), "[[int!]]", false, null)]
        [TestCase(typeof(IEnumerable<IEnumerable<int>>), "int!", false, new GTW[] { GTW.IsNotNull })]
        public void GenerateTypeExpression(
            Type type,
            string expectedExpression,
            bool definesDefaultValue,
            GTW[] wrappers)
        {
            var mock = new Mock<IGraphTypeExpressionDeclaration>();
            mock.Setup(x => x.HasDefaultValue).Returns(definesDefaultValue);
            mock.Setup(x => x.TypeWrappers).Returns(wrappers);

            var typeExpression = GraphValidation.GenerateTypeExpression(type, mock.Object);
            Assert.AreEqual(expectedExpression, typeExpression.ToString());
        }
    }
}