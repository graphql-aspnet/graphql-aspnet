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
        public async Task Directive_DeclaredOnField_IsApplied()
        {
            var server = new TestServerBuilder(TestOptions.UseCodeDeclaredNames)
                .AddGraphType<TestPerson>()
                .AddGraphType<ToUpperDirective>()
                .Build();

            var person = new TestPerson()
            {
                Name = "big john",
                LastName = "Smith",
            };

            var builder = server.CreateFieldContextBuilder<TestPerson>(
                nameof(TestPerson.Name),
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