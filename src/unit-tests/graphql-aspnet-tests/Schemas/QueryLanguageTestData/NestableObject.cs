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
    public class NestableObject
    {
        public NestableObject()
        {
        }

        public NestableObject(int value, NestableObject next = null)
        {
            this.Value = value;
            this.Next = next;
        }

        public int Value { get; set; }

        public NestableObject Next { get; set; }
    }
}