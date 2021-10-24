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
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;

    [GraphType(InputName = "OtherName")]
    public class GenericObjectTypeWithGraphTypeName<T1, T2> : TwoPropertyGenericObject<T1, T2>
    {
    }
}