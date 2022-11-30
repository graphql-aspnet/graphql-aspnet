// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.TestData.IntrospectionTestData
{
    using System.ComponentModel;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;

    public class TypeExtensionDescriptionController : GraphController
    {
        [QueryRoot]
        public TwoPropertyObject RetrieveObject(string prop1Value)
        {
            return new TwoPropertyObject()
            {
                Property1 = prop1Value,
                Property2 = prop1Value.Length,
            };
        }

        [Description("Property3 is a boolean")]
        [TypeExtension(typeof(TwoPropertyObject), "property3", typeof(bool))]
        public bool TwoProp_Property3(TwoPropertyObject obj)
        {
            return obj.Property1 == "bob";
        }
    }
}