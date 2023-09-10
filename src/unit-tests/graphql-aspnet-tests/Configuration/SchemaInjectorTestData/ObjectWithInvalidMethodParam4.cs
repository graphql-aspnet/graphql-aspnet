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

    public class ObjectWithInvalidMethodParam4
    {
        // this method doesnt' represent a valid signature for any schema
        public int InvalidTestMethod(Func<TwoPropertyObject, bool> param1)
        {
            return 1;
        }

        public int Prop1 { get; set; }
    }
}