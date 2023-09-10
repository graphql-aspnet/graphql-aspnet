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

    public class ObjectWithInvalidMethodParam2
    {
        public int InvalidTestMethod(Action<TwoPropertyObject, bool> param1)
        {
            return 1;
        }

        public int Prop1 { get; set; }
    }
}