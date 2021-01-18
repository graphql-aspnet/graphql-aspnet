// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.InputVariableExecutionTestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Tests.CommonHelpers;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;

    [GraphRoot]
    public class InputValueController : GraphController
    {
        [Query("scalarValue")]
        public string SingleValue(string arg1 = "defaultArg1")
        {
            return arg1;
        }

        [Query("complexValue")]
        public TwoPropertyObject ComplexValue(TwoPropertyObject arg1 = null)
        {
            return arg1;
        }
    }
}