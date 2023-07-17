// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution
{
    using System.Linq;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Execution.RulesEngine.RuleSets.DocumentValidation.QueryFragmentSteps;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.Generation.TypeTemplates;
    using GraphQL.AspNet.Tests.Execution.ExecutionArgumentTestData;
    using GraphQL.AspNet.Tests.Framework;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class ExecutionArgumentTests
    {
        private ExecutionArgumentCollection CreateArgumentCollection(string key, object value)
        {
            var argSet = new ExecutionArgumentCollection();

            var mockFieldArg = new Mock<IGraphArgument>();
            mockFieldArg.Setup(x => x.ParameterName).Returns(key);

            argSet.Add(new ExecutionArgument(mockFieldArg.Object, value));
            return argSet;
        }

        [Test]
        public void TryGetArgument_WhenArgExists_AndIsCastable_Succeeds()
        {
            var col = CreateArgumentCollection("test1", "string1");

            var success = col.TryGetArgument<string>("test1", out var value);
            Assert.IsTrue(success);
            Assert.AreEqual("string1", value);
        }

        [Test]
        public void TryGetArgument_WhenArgDoesntExists_FailsWithResponse()
        {
            var col = CreateArgumentCollection("test1", "string1");

            var success = col.TryGetArgument<string>("test2", out var value);
            Assert.IsFalse(success);
            Assert.AreEqual(null, value);
        }

        [Test]
        public void TryGetArgument_WhenArgDoesntCast_FailsWithResponse()
        {
            var col = CreateArgumentCollection("test1", "string1");

            var success = col.TryGetArgument<int>("test1", out var value);
            Assert.IsFalse(success);
            Assert.AreEqual(0, value); // default of int
        }

        [Test]
        public void PrepareArguments_WhenArgumentShouldComeFromDI_AndExistsInDI_IsPreparedCorrectly()
        {
            var serviceInstance = new ServiceForExecutionArgumentTest();

            var builder = new TestServerBuilder(TestOptions.UseCodeDeclaredNames);
            builder.AddGraphQL(o =>
                {
                    o.AddType<ObjectWithFields>();
                    o.DeclarationOptions.ArgumentBindingRule = SchemaArgumentBindingRules.ParametersPreferQueryResolution;
                });
            builder.AddSingleton<IServiceForExecutionArgumentTest>(serviceInstance);

            var testServer = builder.Build();

            var contextBuilder = testServer
                .CreateGraphTypeFieldContextBuilder<ObjectWithFields>(nameof(ObjectWithFields.FieldWithInjectedArgument));

            var context = contextBuilder.CreateResolutionContext();

            // mimic a situation where no values are parsed from a query (no execution args)
            var argSet = new ExecutionArgumentCollection() as IExecutionArgumentCollection;
            argSet = argSet.ForContext(context);

            var resolvedArgs = argSet.PrepareArguments(contextBuilder.ResolverMetaData.Object);

            Assert.IsNotNull(resolvedArgs);
            Assert.AreEqual(1, resolvedArgs.Length);
            Assert.AreEqual(resolvedArgs[0], serviceInstance);
        }

        [Test]
        public void PrepareArguments_WhenArgumentShouldComeFromDI_AndDoesNotExistInDI_AndHasNoDefaultValue_ExecutionExceptionIsThrown()
        {
            var serviceInstance = new ServiceForExecutionArgumentTest();

            var builder = new TestServerBuilder(TestOptions.UseCodeDeclaredNames);
            builder.AddGraphQL(o =>
            {
                o.AddType<ObjectWithFields>();
                o.DeclarationOptions.ArgumentBindingRule = SchemaArgumentBindingRules.ParametersPreferQueryResolution;
            });

            var testServer = builder.Build();

            var contextBuilder = testServer
                .CreateGraphTypeFieldContextBuilder<ObjectWithFields>(nameof(ObjectWithFields.FieldWithInjectedArgument));

            var context = contextBuilder.CreateResolutionContext();

            // mimic a situation where no values are parsed from a query (no execution args)
            // and no default value is present
            var argSet = new ExecutionArgumentCollection() as IExecutionArgumentCollection;
            argSet = argSet.ForContext(context);

            Assert.Throws<GraphExecutionException>(() =>
            {
                var resolvedArgs = argSet.PrepareArguments(contextBuilder.ResolverMetaData.Object);
            });
        }

        [Test]
        public void PrepareArguments_WhenArgumentShouldComeFromDI_AndDoesNotExistInDI_AndHasADefaultValue_ValueIsResolved()
        {
            var serviceInstance = new ServiceForExecutionArgumentTest();

            var builder = new TestServerBuilder(TestOptions.UseCodeDeclaredNames);
            builder.AddGraphQL(o =>
            {
                o.AddType<ObjectWithFields>();
                o.DeclarationOptions.ArgumentBindingRule = SchemaArgumentBindingRules.ParametersPreferQueryResolution;
            });

            var testServer = builder.Build();

            var contextBuilder = testServer
                .CreateGraphTypeFieldContextBuilder<ObjectWithFields>(nameof(ObjectWithFields.FieldWithInjectedArgumentWithDefaultValue));

            // mimic a situation where no values are parsed from a query (no execution args)
            // and nothing is available from DI
            // but the parameter declares a default value
            var argSet = new ExecutionArgumentCollection() as IExecutionArgumentCollection;
            argSet = argSet.ForContext(contextBuilder.CreateResolutionContext());

            var resolvedArgs = argSet.PrepareArguments(contextBuilder.ResolverMetaData.Object);
            Assert.IsNotNull(resolvedArgs);
            Assert.AreEqual(1, resolvedArgs.Length);
            Assert.IsNull(resolvedArgs[0]);
        }

        [Test]
        public void PrepareArguments_WhenArgumentShouldComeFromSchema_HasNoSuppliedValueHasNoDefaultValueIsNullable_ValueIsResolved()
        {
            var serviceInstance = new ServiceForExecutionArgumentTest();

            var builder = new TestServerBuilder(TestOptions.UseCodeDeclaredNames);
            builder.AddGraphQL(o =>
            {
                o.AddType<ObjectWithFields>();
                o.DeclarationOptions.ArgumentBindingRule = SchemaArgumentBindingRules.ParametersPreferQueryResolution;
            });

            var testServer = builder.Build();

            var template = new ObjectGraphTypeTemplate(typeof(ObjectWithFields));
            template.Parse();
            template.ValidateOrThrow();

            var fieldBuilder = testServer
                .CreateGraphTypeFieldContextBuilder<ObjectWithFields>(nameof(ObjectWithFields.FieldWithNullableSchemaArgument));

            // mimic a situation where no values are parsed from a query (no execution args)
            // and nothing is available from DI
            // but the parameter declares a default value
            var argSet = new ExecutionArgumentCollection() as IExecutionArgumentCollection;
            argSet = argSet.ForContext(fieldBuilder.CreateResolutionContext());

            var resolvedArgs = argSet.PrepareArguments(fieldBuilder.ResolverMetaData.Object);
            Assert.IsNotNull(resolvedArgs);
            Assert.AreEqual(1, resolvedArgs.Length);
            Assert.IsNull(resolvedArgs[0]);
        }

        [Test]
        public void PrepareArguments_WhenArgumentShouldComeFromSchema_HasNoSuppliedValueAndIsNotNullableHasDefaultValue_ValueIsResolved()
        {
            var serviceInstance = new ServiceForExecutionArgumentTest();

            var builder = new TestServerBuilder(TestOptions.UseCodeDeclaredNames);
            builder.AddGraphQL(o =>
            {
                o.AddType<ObjectWithFields>();
                o.DeclarationOptions.ArgumentBindingRule = SchemaArgumentBindingRules.ParametersPreferQueryResolution;
            });

            var testServer = builder.Build();

            var fieldBuilder = testServer
                .CreateGraphTypeFieldContextBuilder<ObjectWithFields>(nameof(ObjectWithFields.FieldWithNonNullableSchemaArgumentThatHasDefaultValue));

            // mimic a situation where no values are parsed from a query (no execution args)
            // and nothing is available from DI
            // but the parameter declares a default value
            var argSet = new ExecutionArgumentCollection() as IExecutionArgumentCollection;
            argSet = argSet.ForContext(fieldBuilder.CreateResolutionContext());

            var resolvedArgs = argSet.PrepareArguments(fieldBuilder.ResolverMetaData.Object);
            Assert.IsNotNull(resolvedArgs);
            Assert.AreEqual(1, resolvedArgs.Length);
            Assert.AreEqual(3, resolvedArgs[0]);
        }

        [Test]
        public void PrepareArguments_WhenArgumentShouldComeFromQuery_AndIsNotSupplied_AndIsNotNullable_ExecutionExceptionOccurs()
        {
            var serviceInstance = new ServiceForExecutionArgumentTest();

            var builder = new TestServerBuilder(TestOptions.UseCodeDeclaredNames);
            builder.AddGraphQL(o =>
            {
                o.AddType<ObjectWithFields>();
                o.DeclarationOptions.ArgumentBindingRule = SchemaArgumentBindingRules.ParametersPreferQueryResolution;
            });

            var testServer = builder.Build();

            var fieldBuilder = testServer
                .CreateGraphTypeFieldContextBuilder<ObjectWithFields>(nameof(ObjectWithFields.FieldWithNotNullableQueryArgument));

            // mimic a situation where no values are parsed from a query (no execution args)
            // but the argument expected there to be
            var argSet = new ExecutionArgumentCollection() as IExecutionArgumentCollection;
            argSet = argSet.ForContext(fieldBuilder.CreateResolutionContext());

            Assert.Throws<GraphExecutionException>(() =>
            {
                var resolvedArgs = argSet.PrepareArguments(fieldBuilder.ResolverMetaData.Object);
            });
        }
    }
}