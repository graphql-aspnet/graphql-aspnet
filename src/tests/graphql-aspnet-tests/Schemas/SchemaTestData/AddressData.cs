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

    public class AddressData
    {
        [GraphField]
        public string Address1 { get; set; }

        [GraphField]
        public string Address2 { get; set; }

        [GraphField]
        public string City { get; set; }

        [GraphField]
        public string State { get; set; }

        [GraphField]
        public string PostalCode { get; set; }

        [GraphField]
        public float PhoneNumber { get; set; }

        [GraphField]
        public CountryData Country { get; set; }
    }
}