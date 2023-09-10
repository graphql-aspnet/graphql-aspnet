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

    public delegate void MyDelegate(int param1);

    public class ObjectWithInvalidMethodParam5
    {
        // a custom delegate
        public int InvalidTestMethod(MyDelegate param1)
        {
            return 1;
        }

        public int Prop1 { get; set; }
    }
}