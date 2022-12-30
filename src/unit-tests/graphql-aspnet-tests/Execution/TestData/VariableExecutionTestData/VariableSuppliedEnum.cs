// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.TestData.VariableExecutionTestData
{
    using System;

    [Flags]
    public enum VariableSuppliedEnum
    {
        Value0 = 0,
        Value1 = 1,
        Value2 = 2,
        Value3 = Value1 | Value2,
    }
}