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
    public class Person
    {
        private string _testName;

        public Person()
        {
            _testName = nameof(Person);
        }

        public Person(string testName)
        {
            _testName = testName;
        }

        public string Name { get; set; }

        public Person Child { get; set; }

        public override string ToString()
        {
            return _testName;
        }
    }
}