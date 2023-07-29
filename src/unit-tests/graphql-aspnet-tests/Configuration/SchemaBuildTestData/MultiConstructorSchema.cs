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

    public class MultiConstructorSchema : GraphSchema
    {
        public MultiConstructorSchema(TestService1 service)
        {
            ArgumentNullException.ThrowIfNull(service, nameof(service));
            this.PropValue = 1;
        }

        public MultiConstructorSchema(TestService1 service, TestService2 service2)
        {
            ArgumentNullException.ThrowIfNull(service, nameof(service));
            ArgumentNullException.ThrowIfNull(service2, nameof(service2));
            this.PropValue = 2;
        }

        public MultiConstructorSchema(TestService1 service, TestService2 service2, TestService3 service3)
        {
            ArgumentNullException.ThrowIfNull(service, nameof(service));
            ArgumentNullException.ThrowIfNull(service2, nameof(service2));
            ArgumentNullException.ThrowIfNull(service3, nameof(service3));
            this.PropValue = 3;
        }

        public int PropValue { get; set; }
    }
}