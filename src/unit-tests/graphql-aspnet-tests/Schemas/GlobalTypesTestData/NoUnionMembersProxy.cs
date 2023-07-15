// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Schemas.GlobalTypesTestData
{
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Common.CommonHelpers;

    public class NoUnionMembersProxy : GraphUnionProxy
    {
        public NoUnionMembersProxy()
        {
            this.Name = "Name";
        }
    }
}