// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Schemas.SchemaTestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;

    [GraphRoute("path0")]
    public class NestedMutationMethodController : GraphController
    {
        [Mutation]
        public PersonData AddPerson(string firstName, string lastName, int age, decimal wage)
        {
            return new PersonData();
        }
    }
}