// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.Variables
{
    using GraphQL.AspNet.Execution.Variables;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Resolvables;
    using GraphQL.AspNet.Interfaces.Execution.Variables;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class InputFieldSetVariableTests
    {
        [Test]
        public void TryGetField_ForFoundVariable_IsTrue()
        {
            var mockvar = new Mock<IInputVariable>();
            mockvar.Setup(x => x.Name).Returns("var1Data");

            var variable = new InputFieldSetVariable("var1");
            variable.AddVariable(mockvar.Object);

            var found = ((IResolvableFieldSet)variable).TryGetField("var1Data", out var fieldOut);

            Assert.IsTrue((bool)found);
            Assert.AreEqual(mockvar.Object, fieldOut);
        }

        [Test]
        public void TryGetField_ForNotFoundVariable_IsFalse()
        {
            var variable = new InputFieldSetVariable("var1");

            var found = ((IResolvableFieldSet)variable).TryGetField("var1Data", out var fieldOut);

            Assert.IsFalse((bool)found);
            Assert.IsNull(fieldOut);
        }
    }
}