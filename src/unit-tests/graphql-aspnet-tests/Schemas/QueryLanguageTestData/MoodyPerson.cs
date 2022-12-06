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
    public class MoodyPerson
    {
        private string _testName;

        public MoodyPerson()
        {
            _testName = nameof(MoodyPerson);
        }

        public MoodyPerson(string testName)
        {
            _testName = testName;
        }

        public string Name { get; set; }

        public Happiness HappinessLevel { get; set; }

        public override string ToString()
        {
            return _testName;
        }
    }
}