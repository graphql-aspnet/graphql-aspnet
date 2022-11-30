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
    using System;

    public class TestSlug
    {
        public DateTime DateTime { get; set; }

        public DateTimeOffset DateTimeOffset { get; set; }

        public int Int { get; set; }

        public double Double { get; set; }

        public Uri Uri { get; set; }

        public GraphId GraphId { get; set; }

        public string String { get; set; }
    }
}