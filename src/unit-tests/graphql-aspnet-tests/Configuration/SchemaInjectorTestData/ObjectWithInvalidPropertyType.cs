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
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using GraphQL.AspNet.Tests.Framework.Interfaces;

    public class ObjectWithInvalidPropertyType
    {
        // interfaces aren't allowed
        public Func<int> InvalidProperty { get; set; }
    }
}