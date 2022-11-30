// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.QueryPlans
{
    using System.Linq;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document.Parts;
    using GraphQL.AspNet.Tests.Execution.QueryPlans.PlanGenerationTestData;
    using GraphQL.AspNet.Tests.Framework;
    using NUnit.Framework;

    [TestFixture]
    public class InputValueCoersionTests
    {
        [Test]
        public void SingleValueProvidedForAList_IsCoercible()
        {
            var server = new TestServerBuilder()
                .AddType<InputCoersionValidatorController>()
                .Build();

            // arg1 accepts [int] (an array of ints), single value should be coercable into a list
            // https://graphql.github.io/graphql-spec/October2021/#sec-Type-System.List
            var document = server.CreateDocument("query { singleScalarIntInput(arg1: 5) {name} }");

            Assert.IsNotNull(document);
            Assert.AreEqual(0, document.Messages.Count);
            Assert.AreEqual(1, document.Operations.Count);

            var operation = document.Operations[string.Empty];
            Assert.IsNotNull(operation);

            Assert.AreEqual(1, Enumerable.Count(operation.FieldSelectionSet.ExecutableFields));
            var field = operation.FieldSelectionSet.ExecutableFields[0];
            Assert.AreEqual("singleScalarIntInput", field.Name.ToString());

            Assert.AreEqual(1, field.Arguments.Count);

            // value will not be a list at this stage, thats later
            // rules should be enforced such that this is acceptable for a list
            var arg1 = field.Arguments["arg1"];
            var scalarValue = arg1.Value as IScalarSuppliedValue;
            Assert.IsNotNull(scalarValue);
            Assert.AreEqual("5", scalarValue.Value.ToString());
        }
    }
}