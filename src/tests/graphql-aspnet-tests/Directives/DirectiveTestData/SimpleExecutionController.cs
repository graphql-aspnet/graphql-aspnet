// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Directives.DirectiveTestData
{
    using System;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;

    [GraphRoute("simple")]
    public class SimpleExecutionController : GraphController
    {
        [Query]
        public TwoPropertyObject SimpleQueryMethod(string arg1 = "default string", long arg2 = 5)
        {
            return new TwoPropertyObject()
            {
                Property1 = arg1,
                Property2 = arg2 > int.MaxValue ? int.MaxValue : Convert.ToInt32(arg2),
            };
        }

        [MutationRoot]
        public TwoPropertyObject ChangeData(int id)
        {
            return new TwoPropertyObject()
            {
                Property1 = "str",
                Property2 = 5,
            };
        }
    }
}