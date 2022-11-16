// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Variables.ResolvedVariableTestData
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;

    public class VariableTestController : GraphController
    {
        [QueryRoot]
        public string SingleObjectTest(VariableTestObject arg1)
        {
            return null;
        }

        [QueryRoot]
        public string NestedObjectTest(NestedVariableTestObject arg1)
        {
            return null;
        }

        [QueryRoot]
        public string SuperNestedObjectTest(SuperNestedVariableTestObject arg1, SuperNestedVariableTestObject arg2)
        {
            return null;
        }

        [QueryRoot]
        public string ListAsPropertyOfItem(VariableListObject arg1)
        {
            return null;
        }

        [QueryRoot]
        public string ListAsDirectInput(IEnumerable<int> arg1)
        {
            return null;
        }

        [QueryRoot]
        public string ListOfListAsDirectInput(IEnumerable<IEnumerable<int>> arg1)
        {
            return null;
        }

        [QueryRoot]
        public string ListOfListAlternateDeclarationAsDirectInput(IEnumerable<List<int>> arg1)
        {
            return null;
        }

        [QueryRoot]
        public string ListOfListV4(IEnumerable<List<IEnumerable<List<int>>>> arg1)
        {
            return null;
        }
    }
}