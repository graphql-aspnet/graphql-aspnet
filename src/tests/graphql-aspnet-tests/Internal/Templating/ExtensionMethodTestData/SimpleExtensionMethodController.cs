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
    using GraphQL.AspNet.Tests.CommonHelpers;

    public class SimpleExtensionMethodController : GraphController
    {
        [Description("ExtensionDescription")]
        [TypeExtension(typeof(TwoPropertyObject), "Property3", typeof(TwoPropertyObjectV2))]
        public IGraphActionResult TestExtensionMethod(TwoPropertyObject sourceData, int arg1)
        {
            return null;
        }

        [Description("ExtensionDescription")]
        [BatchTypeExtension(typeof(TwoPropertyObject), "Property3", typeof(List<int>))]
        public IGraphActionResult BatchTestExtension(IEnumerable<TwoPropertyObject> sourceData, int arg1)
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
        public IGraphActionResult NoSourceDataParam(int arg1)
        {
            return null;
        }

        [BatchTypeExtension(typeof(TwoPropertyObject), "Property3", typeof(List<int>))]
        public IGraphActionResult NoSourceDataParamA(int arg1)
        {
            return null;
        }
    }
}