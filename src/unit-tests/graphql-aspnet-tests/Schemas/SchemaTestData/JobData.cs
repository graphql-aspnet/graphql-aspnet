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

    public class JobData
    {
        [GraphField]
        public decimal Wage { get; set; }

        [GraphField]
        public uint JobId { get; set; }

        [GraphField]
        public string Title { get; set; }

        [GraphField]
        public string Company { get; set; }
    }
}