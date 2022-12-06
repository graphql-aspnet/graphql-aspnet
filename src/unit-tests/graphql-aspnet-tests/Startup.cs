// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests
{
    using NUnit.Framework;

    [SetUpFixture]
    public class Startup
    {
        [OneTimeSetUp]
        public void Setup()
        {
            GraphQLProviders.TemplateProvider.CacheTemplates = false;
        }
    }
}