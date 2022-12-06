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

    public class CountryData
    {
        [GraphField]
        public float? ExchangeRate { get; set; }

        [GraphField]
        public string CountryCode { get; set; }

        [GraphField]
        public string CountryName { get; set; }
    }
}