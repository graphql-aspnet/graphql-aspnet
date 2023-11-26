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
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Tests.Common.CommonHelpers;

    public class ControllerWithDoubleDeclaredUnion : GraphController
    {
        [Query("field", "myUnion", typeof(TwoPropertyObject))]
        [Union("myUnion", typeof(TwoPropertyObject), typeof(TwoPropertyObjectV2))]
        public IGraphActionResult UnionWithMixedTypeDeclaration()
        {
            return this.Ok(null);
        }
    }
}