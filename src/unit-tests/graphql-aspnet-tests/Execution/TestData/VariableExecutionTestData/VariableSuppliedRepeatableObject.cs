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
    public class VariableSuppliedRepeatableObject
    {
        public string DataField { get; set; }

        public VariableSuppliedRepeatableObject DataObject { get; set; }

        public VariableSuppliedRepeatableObject SecondDataObject { get; set; }
    }
}