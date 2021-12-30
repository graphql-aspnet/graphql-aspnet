// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Execution.TypeSystemDirectiveTests
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Interfaces.Controllers;

    public class PersonController : GraphController
    {
        [QueryRoot(typeof(TestPerson))]
        public IGraphActionResult MakePerson()
        {
            var person = new TestPerson()
            {
                Name = "big john",
                LastName = "Smith",
            };

            return this.Ok(person);
        }
    }
}