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
    public class SuperNestedVariableTestObject
    {
        public NestedVariableTestObject NestedObject1Property { get; set; }

        public NestedVariableTestObject NestedObject2Property { get; set; }

        public VariableTestObject FlatObject1Property { get; set; }

        public VariableTestObject FlatObject2Property { get; set; }
    }
}