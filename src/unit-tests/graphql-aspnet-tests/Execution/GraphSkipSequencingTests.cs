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
    using GraphQL.AspNet.Tests.Execution.TestData.GraphSkipSequencingTestData;
    using GraphQL.AspNet.Tests.Framework;
    using NUnit.Framework;

    [TestFixture]
    public class GraphSkipSequencingTests
    {
        [Test]
        public void SkipFieldWithSameNameAsAnotherField_OBJECT()
        {
            using var restore = new GraphQLGlobalRestorePoint(true);

            var server = new TestServerBuilder()
                  .AddGraphQL(o =>
                  {
                      o.AddType<ClassWithRenamedField>(AspNet.Schemas.TypeSystem.TypeKind.OBJECT);
                  })
                  .Build();
        }

        [Test]
        public void SkipFieldWithSameNameAsAnotherField_INPUTOBJECT()
        {
            using var restore = new GraphQLGlobalRestorePoint(true);

            var server = new TestServerBuilder()
                  .AddGraphQL(o =>
                  {
                      o.AddType<ClassWithRenamedField>(AspNet.Schemas.TypeSystem.TypeKind.INPUT_OBJECT);
                  })
                  .Build();
        }
    }
}
