// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Schemas.QueryLanguageTestData
{
    public class GrandParent
    {
        private string _testName;

        public GrandParent()
        {
            _testName = nameof(GrandParent);
        }

        public GrandParent(string testName)
        {
            _testName = testName;
        }

        public string Name { get; set; }

        public Person Child1 { get; set; }

        public Person Child2 { get; set; }

        public override string ToString()
        {
            return _testName;
        }
    }
}