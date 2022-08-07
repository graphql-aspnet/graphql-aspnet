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
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;

    [GraphRoot]
    public class InputValueController : GraphController
    {
        [Query("scalarValue")]
        public string SingleValue(string arg1 = "defaultArg1")
        {
            return arg1;
        }

        [Query("sumListValues")]
        public int SumListValues(List<int> arg1)
        {
            return arg1.Sum();
        }

        [Query("sumListListValues")]
        public int SumListValues(List<IEnumerable<int>> arg1)
        {
            return arg1.SelectMany(x => x).Sum();
        }

        [Query("stupidDeepListValues")]
        public int SumListValues(List<IEnumerable<List<List<List<List<List<int>>>>>>> arg1)
        {
            return arg1
                .SelectMany(x => x)
                .SelectMany(x => x)
                .SelectMany(x => x)
                .SelectMany(x => x)
                .SelectMany(x => x)
                .SelectMany(x => x).Sum();
        }

        [Query("complexValue")]
        public TwoPropertyObject ComplexValue(TwoPropertyObject arg1 = null)
        {
            return arg1;
        }
    }
}