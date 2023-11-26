// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.RulesEngine
{
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution.Source;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Execution.RulesEngine;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Common.CommonHelpers;
    using GraphQL.AspNet.Tests.Execution.RulesEngine.DirectiveTestData;
    using GraphQL.AspNet.Tests.Framework;
    using NSubstitute;
    using NUnit.Framework;

    [TestFixture]
    public class DirectiveValidationRuleCheckTests
    {
        [Test]
        public void UnknownLocation_FailsValidation()
        {
            var server = new TestServerBuilder()
                .AddType<ObjectTypeDirective>()
                .Build();

            var obj = Substitute.For<IObjectGraphType>();

            var context = server.CreateDirectiveContextBuilder<ObjectTypeDirective>(
                DirectiveLocation.NONE,
                obj)

                .CreateExecutionContext();

            var ruleSet = new DirectiveValidationRuleProcessor();
            var complete = ruleSet.Execute(context.AsEnumerable());

            Assert.IsFalse((bool)complete);
            Assert.IsFalse(context.Messages.IsSucessful);
            Assert.AreEqual(1, context.Messages.Count);
        }

        [Test]
        public void UnknownPhase_FailsValidation()
        {
            var server = new TestServerBuilder()
                .AddType<ObjectTypeDirective>()
                .Build();
            var obj = Substitute.For<IObjectGraphType>();

            var context = server.CreateDirectiveContextBuilder<ObjectTypeDirective>(
                DirectiveLocation.OBJECT,
                obj,
                DirectiveInvocationPhase.Unknown)
                .CreateExecutionContext();

            var ruleSet = new DirectiveValidationRuleProcessor();
            var complete = ruleSet.Execute(context.AsEnumerable());

            Assert.IsFalse((bool)complete);
            Assert.IsFalse(context.Messages.IsSucessful);
            Assert.AreEqual(1, context.Messages.Count);
        }

        [Test]
        public void LocationMisMatch_FailsValidation()
        {
            var server = new TestServerBuilder()
                .AddType<ObjectTypeDirective>()
                .Build();
            var obj = Substitute.For<IObjectGraphType>();

            var context = server.CreateDirectiveContextBuilder<ObjectTypeDirective>(
                DirectiveLocation.FIELD,
                obj,
                DirectiveInvocationPhase.SchemaGeneration)
                .CreateExecutionContext();

            var ruleSet = new DirectiveValidationRuleProcessor();
            var complete = ruleSet.Execute(context.AsEnumerable());

            Assert.IsFalse((bool)complete);
            Assert.IsFalse(context.Messages.IsSucessful);
            Assert.AreEqual(1, context.Messages.Count);
            Assert.AreEqual("5.7.2", context.Messages[0].MetaData[Constants.Messaging.REFERENCE_RULE_NUMBER_KEY]);
        }

        [Test]
        public void NotADirective_FailsValidation()
        {
            var server = new TestServerBuilder()
                .AddType<TwoPropertyObject>()
                .Build();
            var obj = Substitute.For<IObjectGraphType>();

            var context = server.CreateDirectiveContextBuilder<TwoPropertyObject>(
                DirectiveLocation.OBJECT,
                obj,
                DirectiveInvocationPhase.SchemaGeneration)
                .CreateExecutionContext();

            var ruleSet = new DirectiveValidationRuleProcessor();
            var complete = ruleSet.Execute(context.AsEnumerable());

            Assert.IsFalse((bool)complete);
            Assert.IsFalse(context.Messages.IsSucessful);
            Assert.AreEqual(1, context.Messages.Count);
            Assert.AreEqual("5.7.1", context.Messages[0].MetaData[Constants.Messaging.REFERENCE_RULE_NUMBER_KEY]);
        }

        [Test]
        public void ValidateRequest_PassedValidation()
        {
            var server = new TestServerBuilder()
                .AddType<ObjectTypeDirectiveWithParams>()
                .Build();
            var obj = Substitute.For<IObjectGraphType>();

            var context = server.CreateDirectiveContextBuilder<ObjectTypeDirectiveWithParams>(
                DirectiveLocation.OBJECT,
                obj,
                DirectiveInvocationPhase.SchemaGeneration,
                SourceOrigin.None,
                new object[] { 5, "someValue" })
                .CreateExecutionContext();

            var ruleSet = new DirectiveValidationRuleProcessor();
            var complete = ruleSet.Execute(context.AsEnumerable());

            Assert.IsTrue((bool)complete);
            Assert.IsTrue(context.Messages.IsSucessful);
            Assert.AreEqual(0, context.Messages.Count);
        }

        [Test]
        public void IncorrectNumberOfArguments_FailsValidation()
        {
            var server = new TestServerBuilder()
                .AddType<ObjectTypeDirectiveWithParams>()
                .Build();

            var obj = Substitute.For<IObjectGraphType>();

            var context = server.CreateDirectiveContextBuilder<ObjectTypeDirectiveWithParams>(
                DirectiveLocation.OBJECT,
                obj,
                DirectiveInvocationPhase.SchemaGeneration,
                SourceOrigin.None,
                new object[] { 5 }) // directive requires 2 argument, only 1 supplied
                .CreateExecutionContext();

            var ruleSet = new DirectiveValidationRuleProcessor();
            var complete = ruleSet.Execute(context.AsEnumerable());

            Assert.IsFalse((bool)complete);
            Assert.IsFalse(context.Messages.IsSucessful);
            Assert.AreEqual(1, context.Messages.Count);
            Assert.AreEqual("5.7", context.Messages[0].MetaData[Constants.Messaging.REFERENCE_RULE_NUMBER_KEY]);
        }

        [Test]
        public void InvalidArgument_FailsValidation()
        {
            var server = new TestServerBuilder()
                .AddType<ObjectTypeDirectiveWithParams>()
                .Build();

            var obj = Substitute.For<IObjectGraphType>();

            var context = server.CreateDirectiveContextBuilder<ObjectTypeDirectiveWithParams>(
                DirectiveLocation.OBJECT,
                obj,
                DirectiveInvocationPhase.SchemaGeneration,
                SourceOrigin.None,
                new object[] { "notAInt", "validString" }) // arg 1 should be an int
                .CreateExecutionContext();

            var ruleSet = new DirectiveValidationRuleProcessor();
            var complete = ruleSet.Execute(context.AsEnumerable());

            Assert.IsFalse((bool)complete);
            Assert.IsFalse(context.Messages.IsSucessful);
            Assert.AreEqual(1, context.Messages.Count);
            Assert.AreEqual("5.7", context.Messages[0].MetaData[Constants.Messaging.REFERENCE_RULE_NUMBER_KEY]);
        }
    }
}