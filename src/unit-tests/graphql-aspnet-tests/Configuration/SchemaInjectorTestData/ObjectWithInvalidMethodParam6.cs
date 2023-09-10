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

    public class ObjectWithInvalidMethodParam6
    {
        // interfaces aren't allowed
        public int InvalidTestMethod(ISinglePropertyObject param1)
        {
            return 1;
        }

        public int Prop1 { get; set; }
    }
}