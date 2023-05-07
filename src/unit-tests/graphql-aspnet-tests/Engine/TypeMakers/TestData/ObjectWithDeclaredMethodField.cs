// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Engine.TypeMakers.TestData
{
    using GraphQL.AspNet.Attributes;

    public class ObjectWithDeclaredMethodField
    {
        [GraphField]
        public int FieldOnBaseObject(int item)
        {
            return 0;
        }
    }
}