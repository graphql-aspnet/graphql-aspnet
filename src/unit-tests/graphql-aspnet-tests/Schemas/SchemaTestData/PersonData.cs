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
    using System.Collections.Generic;
    using GraphQL.AspNet.Attributes;

    public class PersonData : IPersonData
    {
        [GraphField]
        public string FirstName { get; set; }

        [GraphField]
        public string LastName { get; set; }

        [GraphField]
        public AddressData Address { get; set; }

        [GraphField]
        public PersonData FavoriteChild { get; set; }

        [GraphField]
        public List<PersonData> Children { get; set; }

        public Dictionary<int, PersonData> ChildrenByAge { get; set; }

        [GraphField]
        public JobData Job { get; set; }

        public int Age { get; set; }
    }
}