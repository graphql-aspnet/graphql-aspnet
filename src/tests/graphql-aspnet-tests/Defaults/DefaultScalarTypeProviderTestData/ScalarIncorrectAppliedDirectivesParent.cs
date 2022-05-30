// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Defaults.DefaultScalarTypeProviderTestData
{
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;

    public class ScalarIncorrectAppliedDirectivesParent : ScalarTestBase
    {
        public class SomeItem : ISchemaItem
        {
            public GraphFieldPath Route { get; }

            public IAppliedDirectiveCollection AppliedDirectives { get; }

            public string Name { get; set; }

            public string Description { get; set; }
        }

        public ScalarIncorrectAppliedDirectivesParent()
        {
            this.AppliedDirectives = new AppliedDirectiveCollection(new SomeItem());
        }
    }
}