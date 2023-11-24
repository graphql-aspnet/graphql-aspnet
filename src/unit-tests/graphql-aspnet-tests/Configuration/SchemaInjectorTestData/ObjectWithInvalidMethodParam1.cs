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
    using GraphQL.AspNet.Tests.Common.CommonHelpers;

    public class ObjectWithInvalidMethodParam1
    {
        // no param func
        public int InvalidTestMethod(Func<TwoPropertyObject> param1)
        {
            return 1;
        }

        public int Prop1 { get; set; }
    }
}