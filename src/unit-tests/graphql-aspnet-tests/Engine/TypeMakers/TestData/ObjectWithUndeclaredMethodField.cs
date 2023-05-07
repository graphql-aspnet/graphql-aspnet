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
    public class ObjectWithUndeclaredMethodField
    {
        public int FieldOnBaseObject(int item)
        {
            return 0;
        }
    }
}