// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Internal.Templating.ExtensionMethodTestData
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;

    public class ExtensionMethodController : GraphController
    {
        public enum BatchEnumType
        {
            Value1,
            Value2,
        }

        [Description("ClassTypeExtensionDescription")]
        [TypeExtension(typeof(TwoPropertyObject), "Property3", typeof(TwoPropertyObjectV2))]
        public IGraphActionResult ClassTypeExtension(TwoPropertyObject sourceData, int arg1)
        {
            return null;
        }

        [Description("ClassBatchExtensionDescription")]
        [BatchTypeExtension(typeof(TwoPropertyObject), "Property3", typeof(List<int>))]
        public IGraphActionResult ClassBatchExtension(IEnumerable<TwoPropertyObject> sourceData, int arg1)
        {
            return null;
        }

        [Description("StructTypeExtensionDescription")]
        [TypeExtension(typeof(TwoPropertyStruct), "Property3", typeof(TwoPropertyObjectV2))]
        public IGraphActionResult StructTypeExtension(TwoPropertyStruct sourceData, int arg1)
        {
            return null;
        }

        [Description("StructBatchExtensionDescription")]
        [BatchTypeExtension(typeof(TwoPropertyStruct), "Property3", typeof(List<int>))]
        public IGraphActionResult StructBatchTestExtension(IEnumerable<TwoPropertyStruct> sourceData, int arg1)
        {
            return null;
        }

        [Description("InterfaceTypeExtensionDescription")]
        [TypeExtension(typeof(ITwoPropertyObject), "Property3", typeof(TwoPropertyObjectV2))]
        public IGraphActionResult InterfaceTypeExtension(ITwoPropertyObject sourceData, int arg1)
        {
            return null;
        }

        [Description("InterfaceBatchExtensionDescription")]
        [BatchTypeExtension(typeof(ITwoPropertyObject), "Property3", typeof(List<int>))]
        public IGraphActionResult InterfaceBatchTestExtension(IEnumerable<ITwoPropertyObject> sourceData, int arg1)
        {
            return null;
        }

        [BatchTypeExtension(typeof(TwoPropertyObject), "Property3")]
        public IDictionary<TwoPropertyObject, List<int>> CustomValidReturnType(IEnumerable<TwoPropertyObject> sourceData, int arg1)
        {
            return null;
        }

        [BatchTypeExtension(typeof(TwoPropertyObject), "Property3", typeof(List<string>))]
        public IDictionary<TwoPropertyObject, List<int>> MismatchedPropertyDeclarations(IEnumerable<TwoPropertyObject> sourceData, int arg1)
        {
            return null;
        }

        [BatchTypeExtension(typeof(TwoPropertyObject), "Property3", typeof(List<int>))]
        public IGraphActionResult BatchExensionNoSourceDataParam(int arg1)
        {
            return null;
        }

        [TypeExtension(typeof(TwoPropertyObject), "Property3", typeof(List<int>))]
        public IGraphActionResult TypeExtensionNoSourceDataParam(int arg1)
        {
            // declared here as a queue for the future so we dont forget we've accounted for it
            // type extensions dont need a source input (its simply ignored)
            return null;
        }

        [TypeExtension(typeof(int), "Property3", typeof(IEnumerable<TwoPropertyObject>))]
        public IGraphActionResult ScalarTypeExtensionFails(int sourceData)
        {
            return null;
        }

        [BatchTypeExtension(typeof(int), "Property3", typeof(IEnumerable<TwoPropertyObject>))]
        public IGraphActionResult ScalarBatchExtensionFails(IEnumerable<int> sourceData)
        {
            return null;
        }

        [TypeExtension(typeof(BatchEnumType), "Property3", typeof(IEnumerable<TwoPropertyObject>))]
        public IGraphActionResult EnumTypeExtensionFails(BatchEnumType sourceData)
        {
            return null;
        }

        [BatchTypeExtension(typeof(BatchEnumType), "Property3", typeof(IEnumerable<TwoPropertyObject>))]
        public IGraphActionResult EnumBatchExtensionFails(IEnumerable<BatchEnumType> sourceData)
        {
            return null;
        }
    }
}