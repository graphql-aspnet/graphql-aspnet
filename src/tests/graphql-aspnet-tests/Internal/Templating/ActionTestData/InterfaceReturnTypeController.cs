// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Internal.Templating.ActionTestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;

    public class InterfaceReturnTypeController : GraphController
    {
        public struct MyStruct
        {
            public int Data1 { get; }
        }

        [Query]
        [PossibleTypes(typeof(TestItemA), typeof(TestItemB))]
        public ITestItem RetrieveData()
        {
            return null;
        }

        [Query]
        public ITestItem RetrieveDataNoAttributeDeclared()
        {
            return null;
        }

        [Query]
        [PossibleTypes(typeof(TwoPropertyObject))]
        public ITestItem RetrieveDataInvalidPossibleType()
        {
            return null;
        }

        [Query]
        [PossibleTypes(typeof(IUnionTestDataItem))]
        public ITestItem RetrieveDataPossibleTypeIsInterface()
        {
            return null;
        }

        [Query]
        [PossibleTypes(typeof(string))]
        public ITestItem RetrieveDataPossibleTypeIsScalar()
        {
            return null;
        }

        [Query]
        [PossibleTypes(typeof(MyStruct))]
        public ITestItem RetrieveDataPossibleTypeIsAStruct()
        {
            return null;
        }
    }
}