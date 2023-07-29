// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Configuration.SchemaBuildTestData
{
    using System;
    using GraphQL.AspNet.Schemas;

    public class OneServiceSchema : GraphSchema
    {
        public OneServiceSchema(TestService1 service)
        {
            ArgumentNullException.ThrowIfNull(service, nameof(service));
        }
    }
}