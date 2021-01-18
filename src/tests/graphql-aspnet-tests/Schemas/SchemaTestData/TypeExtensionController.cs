// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Schemas.SchemaTestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Tests.CommonHelpers;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;

    public class TypeExtensionController : GraphController
    {
        [TypeExtension(typeof(TwoPropertyObject), "extraField", typeof(string))]
        public string RetrieveExtraData()
        {
            return "5";
        }
    }
}