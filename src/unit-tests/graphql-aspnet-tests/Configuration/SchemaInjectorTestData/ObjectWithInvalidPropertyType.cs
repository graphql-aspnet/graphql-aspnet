// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Configuration.SchemaInjectorTestData
{
    using System;

    public class ObjectWithInvalidPropertyType
    {
        // interfaces aren't allowed
        public Func<int> InvalidProperty { get; set; }

        public int Prop1 { get; set; }
    }
}