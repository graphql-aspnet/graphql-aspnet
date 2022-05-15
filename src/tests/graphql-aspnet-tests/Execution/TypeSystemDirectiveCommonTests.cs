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
    using System.Threading.Tasks;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Tests.Execution.TypeSystemDirectiveTests;
    using GraphQL.AspNet.Tests.Framework;
    using NUnit.Framework;

    [TestFixture]
    public class TypeSystemDirectiveCommonTests
    {
        [Test]
        public async Task ExtendAResolver_DirectiveDeclaredByType()
        {
            var server = new TestServerBuilder(TestOptions.UseCodeDeclaredNames)
                .AddGraphType<TestPersonWithResolverExtensionDirectiveByType>()
                .AddGraphType<ToUpperDirective>()
                .Build();

            var person = new TestPersonWithResolverExtensionDirectiveByType()
            {
                Name = "big john",
                LastName = "Smith",
            };

            var context = server.CreateFieldExecutionContext<TestPersonWithResolverExtensionDirectiveByType>(
                nameof(TestPersonWithResolverExtensionDirectiveByType.Name),
                person);

            await server.ExecuteField(context);

            var data = context.Result?.ToString();
            Assert.AreEqual("BIG JOHN", data);
        }

        [Test]
        public async Task ExtendAResolver_DirectiveDeclaredByName()
        {
            var server = new TestServerBuilder(TestOptions.UseCodeDeclaredNames)
                .AddGraphType<TestPersonWithResolverExtensionDirectiveByName>()
                .AddGraphType<ToUpperDirective>()
                .Build();

            var person = new TestPersonWithResolverExtensionDirectiveByName()
            {
                Name = "big john",
                LastName = "Smith",
            };

            var context = server.CreateFieldExecutionContext<TestPersonWithResolverExtensionDirectiveByName>(
                nameof(TestPersonWithResolverExtensionDirectiveByName.Name),
                person);

            await server.ExecuteField(context);

            var data = context.Result?.ToString();
            Assert.AreEqual("BIG JOHN", data);
        }

        [Test]
        public async Task DirectiveDeclaredByName_AndDirectiveHasCustomName_IsFoundAndExecuted()
        {
            // TestPerson references a "ToUpper" directive
            // CustomNamedToUpperDirective has its name explicitly set to "ToUpper"
            var server = new TestServerBuilder(TestOptions.UseCodeDeclaredNames)
                .AddGraphType<TestPersonWithResolverExtensionDirectiveByName>()
                .AddGraphType<CustomNamedToUpperDirective>()
                .Build();

            var person = new TestPersonWithResolverExtensionDirectiveByName()
            {
                Name = "big john",
                LastName = "Smith",
            };

            var context = server.CreateFieldExecutionContext<TestPersonWithResolverExtensionDirectiveByName>(
                nameof(TestPersonWithResolverExtensionDirectiveByName.Name),
                person);

            await server.ExecuteField(context);

            var data = context.Result?.ToString();
            Assert.AreEqual("BIG JOHN", data);
        }

        [Test]
        public async Task ExtendAnObjectType_AddField_DirectiveDeclaredByType()
        {
            var server = new TestServerBuilder(TestOptions.UseCodeDeclaredNames)
                .AddGraphType<TestObjectWithAddFieldDirectiveByType>()
                .AddGraphType<AddFieldDirective>()
                .Build();

            var obj = new TestObjectWithAddFieldDirectiveByType()
            {
                Property1 = "prop 1",
                Property2 = "prop 2",
            };

            var context = server.CreateFieldExecutionContext<TestObjectWithAddFieldDirectiveByType>(
                "property3",
                obj);

            await server.ExecuteField(context);

            var data = context.Result?.ToString();
            Assert.AreEqual("prop 1 property 3", data);
        }
    }
}