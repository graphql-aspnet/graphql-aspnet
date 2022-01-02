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
    using GraphQL.AspNet.Tests.Execution.TypeSystemDirectiveTests;
    using GraphQL.AspNet.Tests.Framework;
    using NUnit.Framework;

    [TestFixture]
    public class TypeSystemDirectiveInvocationTests
    {
        [Test]
        public async Task Directive_ByType_DeclaredOnField_IsApplied()
        {
            var server = new TestServerBuilder(TestOptions.UseCodeDeclaredNames)
                .AddGraphType<TestPersonWithDirectiveType>()
                .AddGraphType<ToUpperDirective>()
                .Build();

            var person = new TestPersonWithDirectiveType()
            {
                Name = "big john",
                LastName = "Smith",
            };

            var builder = server.CreateGraphTypeFieldContextBuilder<TestPersonWithDirectiveType>(
                nameof(TestPersonWithDirectiveType.Name),
                person);

            var context = builder.CreateExecutionContext();
            await server.ExecuteField(context);

            var data = context.Result?.ToString();
            Assert.AreEqual("BIG JOHN", data);
        }

        [Test]
        public async Task Directive_ByName_DeclaredOnField_IsApplied()
        {
            var server = new TestServerBuilder(TestOptions.UseCodeDeclaredNames)
                .AddGraphType<TestPersonWithDirectiveName>()
                .AddGraphType<ToUpperDirective>()
                .Build();

            var person = new TestPersonWithDirectiveName()
            {
                Name = "big john",
                LastName = "Smith",
            };

            var builder = server.CreateGraphTypeFieldContextBuilder<TestPersonWithDirectiveName>(
                nameof(TestPersonWithDirectiveName.Name),
                person);

            var context = builder.CreateExecutionContext();
            await server.ExecuteField(context);

            var data = context.Result?.ToString();
            Assert.AreEqual("BIG JOHN", data);
        }

        // adding a field

        // removing a field

        // renaming a field

        // renaming an input argument
    }
}