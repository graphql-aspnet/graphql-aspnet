// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.PlanGeneration
{
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs.Values;
    using GraphQL.AspNet.PlanGeneration.Document.Parts.QueryInputValues;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.PlanGeneration.PlanGenerationTestData;
    using NUnit.Framework;

    [TestFixture]
    public class InputValueCoersionTests
    {
        [Test]
        public void SingleValueProvidedForAList_IsCoercible()
        {
            var server = new TestServerBuilder()
                .AddGraphType<InputCoersionValidatorController>()
                .Build();

            // arg1 accepts [int] (an array of ints), single value should be coercable into a list
            // https://graphql.github.io/graphql-spec/June2018/#sec-Type-System.List
            var document = server.CreateDocument("query { singleScalarIntInput(arg1: 5) {name} }");

            Assert.IsNotNull(document);
            Assert.AreEqual(0, document.Messages.Count);
            Assert.AreEqual(1, document.Operations.Count);

            var operation = document.Operations[string.Empty];
            Assert.IsNotNull(operation);

            Assert.AreEqual(1, operation.FieldSelectionSet.Count);
            var field = operation.FieldSelectionSet[0];
            Assert.AreEqual("singleScalarIntInput", field.Name.ToString());

            Assert.AreEqual(1, field.Arguments.Count);

            // value will not be a list at this stage, thats later
            // rules should be enforced such that this is acceptable for a list
            var arg1 = field.Arguments["arg1"];
            var scalarValue = arg1.Value as QueryScalarInputValue;
            Assert.IsNotNull(scalarValue);
            Assert.IsTrue(scalarValue.ValueNode is ScalarValueNode);
            Assert.AreEqual("5", scalarValue.Value.ToString());
        }
    }
}