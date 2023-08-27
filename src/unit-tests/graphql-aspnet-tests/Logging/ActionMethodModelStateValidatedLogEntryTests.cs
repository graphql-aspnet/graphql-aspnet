﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Logging
{
    using System;
    using System.Linq;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Logging.LoggerTestData;
    using GraphQL.AspNet.Common.Extensions;
    using NSubstitute;
    using NUnit.Framework;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Controllers.InputModel;
    using GraphQL.AspNet.Logging.GeneralEvents;
    using GraphQL.AspNet.Logging.GeneralEvents.PropertyItems;

    [TestFixture]
    public class ActionMethodModelStateValidatedLogEntryTests
    {
        private ExecutionArgument CreateArgument(
           string name,
           object value,
           Type concreteType = null,
           params MetaGraphTypes[] wrappers)
        {
            concreteType = concreteType ?? value?.GetType() ?? throw new ArgumentException();

            var argTemplate = Substitute.For<IGraphArgument>();

            argTemplate.Name.Returns(name);
            argTemplate.TypeExpression.Returns(new GraphTypeExpression(name, wrappers));
            argTemplate.ArgumentModifiers.Returns(GraphArgumentModifiers.None);
            argTemplate.ObjectType.Returns(concreteType);
            argTemplate.ParameterName.Returns(name);

            return new ExecutionArgument(argTemplate, value);
        }

        private void ValidateModelDictionaryToLogEntry(
            IGraphFieldResolverMethod graphMethod,
            IGraphFieldRequest fieldRequest,
            InputModelStateDictionary dictionary,
            ActionMethodModelStateValidatedLogEntry logEntry)
        {
            Assert.AreEqual(fieldRequest.Id.ToString(), logEntry.PipelineRequestId);
            Assert.AreEqual(graphMethod.Parent.ObjectType.FriendlyName(true), logEntry.ControllerName);
            Assert.AreEqual(graphMethod.Name, logEntry.ActionName);
            Assert.AreEqual(dictionary.IsValid, logEntry.ModelDataIsValid);

            foreach (var kvp in dictionary)
            {
                var itemResult = kvp.Value;

                if (itemResult.ValidationState == InputModelValidationState.Invalid)
                {
                    var entryResult = logEntry.ModelItems.Cast<ModelStateEntryLogItem>().First(x => itemResult.Name == x.Name);

                    Assert.AreEqual(itemResult.Name, entryResult.Name);
                    Assert.AreEqual(itemResult.ValidationState.ToString(), entryResult.ValidationState);

                    var itemErrors = itemResult.Errors;
                    var entryErrors = entryResult.Errors;

                    if (itemErrors.Count == 0)
                    {
                        // log entry should skip the property if
                        // no errors are recorded
                        Assert.IsTrue(entryErrors == null);
                        continue;
                    }

                    Assert.AreEqual(itemErrors.Count, entryErrors.Count);

                    for (var i = 0; i < itemErrors.Count; i++)
                    {
                        var itemError = itemErrors[i];
                        var entryError = entryErrors[i] as ModelStateErrorLogItem;

                        Assert.AreEqual(itemError.ErrorMessage, entryError.ErrorMessage);

                        var itemException = itemError.Exception;
                        var entryException = entryError.Exception as ExceptionLogItem;

                        Assert.AreEqual(itemException == null, entryException == null);
                        if (itemException != null)
                        {
                            Assert.AreEqual(itemException.Message, entryException.ExceptionMessage);
                            Assert.AreEqual(itemException.StackTrace, entryException.StackTrace);
                            Assert.AreEqual(itemException.GetType().FriendlyName(true), entryException.TypeName);
                        }
                    }
                }
            }
        }

        [Test]
        public void InvalidModelItem_BuildsLogEntry()
        {
            var server = new TestServerBuilder(TestOptions.UseCodeDeclaredNames)
                .AddType<LogTestController>()
                .Build();

            var builder = server.CreateGraphTypeFieldContextBuilder<LogTestController>(
                    nameof(LogTestController.ValidatableInputObject));

            var context = builder.CreateExecutionContext();

            // valid range 0 - 35
            var item = new ValidatableObject()
            {
                Age = 99,
            };

            var generator = new ModelStateGenerator();
            var argumentToTest = CreateArgument("testItem", item);
            var dictionary = generator.CreateStateDictionary(argumentToTest);

            var entry = new ActionMethodModelStateValidatedLogEntry(
                builder.GraphMethod,
                context.Request,
                dictionary);

            this.ValidateModelDictionaryToLogEntry(builder.GraphMethod, context.Request, dictionary, entry);
        }

        [Test]
        public void ValidModelItem_BuildsLogEntry()
        {
            var server = new TestServerBuilder(TestOptions.UseCodeDeclaredNames)
                .AddType<LogTestController>()
                .Build();

            var builder = server.CreateGraphTypeFieldContextBuilder<LogTestController>(
                nameof(LogTestController.ValidatableInputObject));

            var context = builder.CreateExecutionContext();

            // valid range 0 - 35
            var item = new ValidatableObject()
            {
                Age = 18,
            };

            var generator = new ModelStateGenerator();
            var argumentToTest = CreateArgument("testItem", item);
            var dictionary = generator.CreateStateDictionary(argumentToTest);

            var entry = new ActionMethodModelStateValidatedLogEntry(
                builder.GraphMethod,
                context.Request,
                dictionary);

            this.ValidateModelDictionaryToLogEntry(builder.GraphMethod, context.Request, dictionary, entry);
        }
    }
}