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

    public class ObjectWithInvalidMethodParam3
    {
        // no return action
        public int InvalidTestMethod(Action param1)
        {
            return 1;
        }

        public int Prop1 { get; set; }
    }
}