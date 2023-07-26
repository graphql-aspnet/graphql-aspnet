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
    using GraphQL.AspNet.Tests.Schemas.Generation.TypeMakers.TestData;

    public class ControllerWithUnionAttributes : GraphController
    {
        [Query]
        [Union("myUnion", typeof(TwoPropertyObject), typeof(TwoPropertyObjectV2))]
        public IGraphActionResult UnionDeclaredViaUnionAttribute()
        {
            return this.Ok(null);
        }

        [Query("field", "myUnion")]
        [PossibleTypes(typeof(TwoPropertyObject), typeof(TwoPropertyObjectV2))]
        public IGraphActionResult UnionWithMixedTypeDeclaration()
        {
            return this.Ok(null);
        }

        [Query("field1", typeof(UnionWithInternalName))]
        public IGraphActionResult UnionViaProxyOnQuery()
        {
            return this.Ok(null);
        }

        [Query]
        [Union(typeof(UnionWithInternalName))]
        public IGraphActionResult UnionViaProxyOnUnionAttribute()
        {
            return this.Ok(null);
        }

        [Query(returnType: null)]
        [Union(typeof(UnionWithInternalName))]
        [PossibleTypes(null, null, null, null)]
        public IGraphActionResult LotsOfNullsWithProxy()
        {
            return this.Ok(null);
        }

        [Query(returnType: typeof(TwoPropertyObject))]
        [Union("myUnion2", typeof(TwoPropertyObjectV2), null, null)]
        [PossibleTypes(null, null, null, null)]
        public IGraphActionResult LotsOfNullsWithDeclaration()
        {
            return this.Ok(null);
        }
    }
}