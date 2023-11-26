// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Schemas.Generation.TypeTemplates.ControllerTestData
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Tests.Schemas.Generation.TypeTemplates.ExtensionMethodTestData;

    public class ControllerWithActionAsTypeExtensionForCustomNamedObject : GraphController
    {
        [BatchTypeExtension(typeof(CustomNamedObject), "fieldThree")]
        public IDictionary<CustomNamedObject, IEnumerable<CustomNamedObject>> ObjExtension(
            IEnumerable<CustomNamedObject> customNamedObjects)
        {
            return null;
        }
    }
}