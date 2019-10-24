// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Controllers
{
    using System;
    using System.Collections.Generic;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.InputModel;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Controllers.ModelStateTestData;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class GraphModelStateDictionaryTests
    {
        private ExecutionArgument CreateArgument(
            string name,
            object value,
            Type concreteType = null,
            params MetaGraphTypes[] wrappers)
        {
            concreteType = concreteType ?? value?.GetType() ?? throw new ArgumentException();

            var argTemplate = new Mock<IGraphFieldArgument>();

            argTemplate.Setup(x => x.Name).Returns(name);
            argTemplate.Setup(x => x.TypeExpression).Returns(new GraphTypeExpression(name, wrappers));
            argTemplate.Setup(x => x.ArgumentModifiers).Returns(GraphArgumentModifiers.None);
            argTemplate.Setup(x => x.ObjectType).Returns(concreteType);
            argTemplate.Setup(x => x.ParameterName).Returns(name);

            return new ExecutionArgument(argTemplate.Object, value);
        }

        [Test]
        public void ScalarValueProperties_ValidModelItem_ReportValidState()
        {
            var item = new ValidatiableScalarPropertyObject()
            {
                NullableProperty = null,
                RangeProperty = 7,
                RequiredLengthProperty = "abc123",
                RequiredProperty = "b",
            };

            var generator = new ModelStateGenerator();
            var argumentToTest = CreateArgument("testItem", item);
            var dictionary = generator.CreateStateDictionary(argumentToTest);

            Assert.IsTrue(dictionary.IsValid);
            Assert.AreEqual(1, dictionary.Count);
            Assert.IsTrue(dictionary.ContainsKey("testItem"));

            var entry = dictionary["testItem"];
            Assert.AreEqual(0, entry.Errors.Count);
        }

        [Test]
        public void ComplexValueProperties_ValidModelItem_ReportValidState()
        {
            var item = new ValidatiableComplexPropertyObject()
            {
                Child = new ValidatiableScalarPropertyObject()
                {
                    NullableProperty = null,
                    RangeProperty = 7,
                    RequiredLengthProperty = "abc123",
                    RequiredProperty = "b",
                },
                RangeValue = 33,
            };

            var generator = new ModelStateGenerator();

            var argumentToTest = CreateArgument("testItem", item);
            var dictionary = generator.CreateStateDictionary(argumentToTest);

            Assert.IsTrue(dictionary.IsValid);
            Assert.AreEqual(1, dictionary.Count);
            Assert.IsTrue(dictionary.ContainsKey("testItem"));

            var entry = dictionary["testItem"];
            Assert.AreEqual(0, entry.Errors.Count);
        }

        [Test]
        public void ScalarValueProperties_InvalidModelItem_ReportInvalidState()
        {
            var item = new ValidatiableScalarPropertyObject()
            {
                NullableProperty = null,
                RangeProperty = -15,  // out of range
                RequiredLengthProperty = "abc123",
                RequiredProperty = "b",
            };

            var generator = new ModelStateGenerator();

            var argumentToTest = CreateArgument("testItem", item);
            var dictionary = generator.CreateStateDictionary(argumentToTest);

            Assert.IsFalse(dictionary.IsValid);
            Assert.AreEqual(1, dictionary.Count);
            Assert.IsTrue(dictionary.ContainsKey("testItem"));

            var entry = dictionary["testItem"];
            Assert.AreEqual(1, entry.Errors.Count);
            Assert.AreEqual("RangeProperty", entry.Errors[0].MemberName);
        }

        [Test]
        public void ComplexValueProperties_InValidChildModelItem_ReportValidState()
        {
            // by default no children properties should checked. so even if rangedproperty is invalid its
            // not checked (requires custom attributes by the developer)
            var item = new ValidatiableComplexPropertyObject()
            {
                Child = new ValidatiableScalarPropertyObject()
                {
                    NullableProperty = null,
                    RangeProperty = -15, // out of range
                    RequiredLengthProperty = "abc123",
                    RequiredProperty = "b",
                },
                RangeValue = 33,
            };
            var generator = new ModelStateGenerator();

            var argumentToTest = CreateArgument("testItem", item);
            var dictionary = generator.CreateStateDictionary(argumentToTest);

            Assert.IsTrue(dictionary.IsValid);
            Assert.AreEqual(1, dictionary.Count);
            Assert.IsTrue(dictionary.ContainsKey("testItem"));
        }

        [Test]
        public void NullAsModelValue_ReportsSkipped()
        {
            var generator = new ModelStateGenerator();

            var argumentToTest = CreateArgument("testItem", null, typeof(ValidatiableScalarPropertyObject));
            var dictionary = generator.CreateStateDictionary(argumentToTest);

            Assert.IsTrue(dictionary.IsValid);
            Assert.AreEqual(1, dictionary.Count);
            Assert.IsTrue(dictionary.ContainsKey("testItem"));

            var entry = dictionary["testItem"];
            Assert.AreEqual(InputModelValidationState.Skipped, entry.ValidationState);
        }

        [Test]
        public void ListAsModelValue_ScalarElementsEachCheckedForValidity_ReportsCorrectly()
        {
            var enumerable = new[] { "string1", "string2", "string3" } as IEnumerable<string>;
            var generator = new ModelStateGenerator();

            var argumentToTest = CreateArgument("testItem", enumerable, typeof(ValidatiableScalarPropertyObject), MetaGraphTypes.IsList);
            var dictionary = generator.CreateStateDictionary(argumentToTest);

            Assert.IsTrue(dictionary.IsValid);
            Assert.AreEqual(1, dictionary.Count);
            Assert.IsTrue(dictionary.ContainsKey("testItem"));
        }
    }
}