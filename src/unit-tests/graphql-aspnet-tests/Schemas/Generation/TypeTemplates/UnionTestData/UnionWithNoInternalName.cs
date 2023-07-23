// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Schemas.Generation.TypeTemplates.UnionTestData
{
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Common.CommonHelpers;

    public class UnionWithNoInternalName : GraphUnionProxy
    {
        public UnionWithNoInternalName()
        {
            this.Name = "ValidUnion";
            this.Description = "My Union Desc";
            this.InternalName = null;
            this.Types.Add(typeof(TwoPropertyObject));
            this.Types.Add(typeof(TwoPropertyObjectV2));
        }
    }
}