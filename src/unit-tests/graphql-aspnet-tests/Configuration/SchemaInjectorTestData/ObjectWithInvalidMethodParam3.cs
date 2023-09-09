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

    public class ObjectWithInvalidMethodParam3
    {
        // no return action
        public int InvalidTestMethod(Action param1)
        {
            return 1;
        }
    }
}