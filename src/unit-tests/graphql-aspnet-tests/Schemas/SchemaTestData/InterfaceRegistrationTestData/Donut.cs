// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Schemas.SchemaTestData.InterfaceRegistrationTestData
{
    public class Donut : IDonut
    {
        public int Flavor { get; set; }

        public string Name { get; set; }
    }
}