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

    public class MultiConstructorSchemWithDefaultValues : GraphSchema
    {
        public MultiConstructorSchemWithDefaultValues(TestService1 service)
        {
            this.PropValue = 1;
        }

        public MultiConstructorSchemWithDefaultValues(TestService1 service, TestService2 service2)
        {
            this.PropValue = 2;
        }

        public MultiConstructorSchemWithDefaultValues(TestService1 service = null, TestService2 service2 = null, TestService3 service3 = null)
        {
            this.PropValue = 3;
        }

        public int PropValue { get; set; }
    }
}