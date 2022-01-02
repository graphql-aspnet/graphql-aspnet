// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.ValidationRuless
{
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Middleware.DirectiveExecution.Components;
    using GraphQL.AspNet.PlanGeneration.InputArguments;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using GraphQL.AspNet.Tests.ValidationRuless.DirectiveTestData;
    using GraphQL.AspNet.ValidationRules;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class DirectiveValidationRuleCheckTests
    {
        private (GraphDirectiveExecutionContext, GraphSchema) CreateContext<TDirective>(
            DirectiveLocation location,
            object directiveTarget,
            DirectiveInvocationPhase phase = DirectiveInvocationPhase.SchemaGeneration,
            SourceOrigin origin = null,
            object[] arguments = null)
            where TDirective : class
        {
            origin = origin ?? SourceOrigin.None;

            var server = new TestServerBuilder()
                .AddGraphType<TDirective>()
                .Build();

            var targetDirective = server.Schema.KnownTypes.FindDirective(typeof(TDirective));

            var operationRequest = new Mock<IGraphOperationRequest>();
            var directiveRequest = new Mock<IGraphDirectiveRequest>();
            var invocationContext = new Mock<IDirectiveInvocationContext>();
            var argCollection = new InputArgumentCollection();

            directiveRequest.Setup(x => x.DirectivePhase).Returns(phase);
            directiveRequest.Setup(x => x.InvocationContext).Returns(invocationContext.Object);
            directiveRequest.Setup(x => x.DirectiveTarget).Returns(directiveTarget);
            directiveRequest.Setup(x => x.Items).Returns(new MetaDataCollection());

            invocationContext.Setup(x => x.Location).Returns(location);
            invocationContext.Setup(x => x.Arguments).Returns(argCollection);
            invocationContext.Setup(x => x.Origin).Returns(origin);
            invocationContext.Setup(x => x.Directive).Returns(targetDirective);

            if (targetDirective != null && targetDirective.Kind == TypeKind.DIRECTIVE
                && arguments != null)
            {
                for (var i = 0; i < targetDirective.Arguments.Count; i++)
                {
                    if (arguments.Length <= i)
                        break;

                    var directiveArg = targetDirective.Arguments[i];
                    var resolvedValue = arguments[i];

                    var argValue = new ResolvedInputArgumentValue(directiveArg.Name, resolvedValue);
                    var inputArg = new InputArgument(directiveArg, argValue);
                    argCollection.Add(inputArg);
                }
            }

                var context = new GraphDirectiveExecutionContext(
                server.Schema,
                server.ServiceProvider,
                operationRequest.Object,
                directiveRequest.Object,
                items: directiveRequest.Object.Items);

            return (context, server.Schema);
        }

        [Test]
        public void UnknownLocation_FailsValidation()
        {
            var obj = new Mock<IObjectGraphType>();

            (var context, var schema) = this.CreateContext<ObjectTypeDirective>(
                DirectiveLocation.NONE,
                obj.Object);

            var ruleSet = new DirectiveValidationRuleProcessor();
            var complete = ruleSet.Execute(context.AsEnumerable());

            Assert.IsFalse(complete);
            Assert.IsFalse(context.Messages.IsSucessful);
            Assert.AreEqual(1, context.Messages.Count);
        }

        [Test]
        public void UnknownPhase_FailsValidation()
        {
            var obj = new Mock<IObjectGraphType>();

            (var context, var schema) = this.CreateContext<ObjectTypeDirective>(
                DirectiveLocation.OBJECT,
                obj.Object,
                DirectiveInvocationPhase.Unknown);

            var ruleSet = new DirectiveValidationRuleProcessor();
            var complete = ruleSet.Execute(context.AsEnumerable());

            Assert.IsFalse(complete);
            Assert.IsFalse(context.Messages.IsSucessful);
            Assert.AreEqual(1, context.Messages.Count);
        }

        [Test]
        public void LocationMisMatch_FailsValidation()
        {
            var obj = new Mock<IObjectGraphType>();

            (var context, var schema) = this.CreateContext<ObjectTypeDirective>(
                DirectiveLocation.FIELD,
                obj.Object,
                DirectiveInvocationPhase.SchemaGeneration);

            var ruleSet = new DirectiveValidationRuleProcessor();
            var complete = ruleSet.Execute(context.AsEnumerable());

            Assert.IsFalse(complete);
            Assert.IsFalse(context.Messages.IsSucessful);
            Assert.AreEqual(1, context.Messages.Count);
            Assert.AreEqual("5.7.2", context.Messages[0].MetaData[Constants.Messaging.REFERENCE_RULE_NUMBER]);
        }

        [Test]
        public void NotADirective_FailsValidation()
        {
            var obj = new Mock<IObjectGraphType>();

            (var context, var schema) = this.CreateContext<TwoPropertyObject>(
                DirectiveLocation.OBJECT,
                obj.Object,
                DirectiveInvocationPhase.SchemaGeneration);

            var ruleSet = new DirectiveValidationRuleProcessor();
            var complete = ruleSet.Execute(context.AsEnumerable());

            Assert.IsFalse(complete);
            Assert.IsFalse(context.Messages.IsSucessful);
            Assert.AreEqual(1, context.Messages.Count);
            Assert.AreEqual("5.7.1", context.Messages[0].MetaData[Constants.Messaging.REFERENCE_RULE_NUMBER]);
        }

        [Test]
        public void ValidateRequest_PassedValidation()
        {
            var obj = new Mock<IObjectGraphType>();

            (var context, var schema) = this.CreateContext<ObjectTypeDirectiveWithParams>(
                DirectiveLocation.OBJECT,
                obj.Object,
                DirectiveInvocationPhase.SchemaGeneration,
                SourceOrigin.None,
                new object[] { 5, "someValue" });

            var ruleSet = new DirectiveValidationRuleProcessor();
            var complete = ruleSet.Execute(context.AsEnumerable());

            Assert.IsTrue(complete);
            Assert.IsTrue(context.Messages.IsSucessful);
            Assert.AreEqual(0, context.Messages.Count);
        }

        [Test]
        public void IncorrectNumberOfArguments_FailsValidation()
        {
            var obj = new Mock<IObjectGraphType>();

            (var context, var schema) = this.CreateContext<ObjectTypeDirectiveWithParams>(
                DirectiveLocation.OBJECT,
                obj.Object,
                DirectiveInvocationPhase.SchemaGeneration,
                SourceOrigin.None,
                new object[] { 5 }); // directive requires 2 argument, only 1 supplied

            var ruleSet = new DirectiveValidationRuleProcessor();
            var complete = ruleSet.Execute(context.AsEnumerable());

            Assert.IsFalse(complete);
            Assert.IsFalse(context.Messages.IsSucessful);
            Assert.AreEqual(1, context.Messages.Count);
            Assert.AreEqual("5.7", context.Messages[0].MetaData[Constants.Messaging.REFERENCE_RULE_NUMBER]);
        }

        [Test]
        public void InvalidArgument_FailsValidation()
        {
            var obj = new Mock<IObjectGraphType>();

            (var context, var schema) = this.CreateContext<ObjectTypeDirectiveWithParams>(
                DirectiveLocation.OBJECT,
                obj.Object,
                DirectiveInvocationPhase.SchemaGeneration,
                SourceOrigin.None,
                new object[] { "notAInt", "validString" }); // arg 1 should be an int

            var ruleSet = new DirectiveValidationRuleProcessor();
            var complete = ruleSet.Execute(context.AsEnumerable());

            Assert.IsFalse(complete);
            Assert.IsFalse(context.Messages.IsSucessful);
            Assert.AreEqual(1, context.Messages.Count);
            Assert.AreEqual("5.7", context.Messages[0].MetaData[Constants.Messaging.REFERENCE_RULE_NUMBER]);
        }
    }
}