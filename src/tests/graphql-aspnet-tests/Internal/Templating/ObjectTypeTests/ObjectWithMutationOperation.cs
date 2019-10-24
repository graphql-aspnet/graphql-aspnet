// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Internal.Templating.ObjectTypeTests
{
    using GraphQL.AspNet.Attributes;

    public class ObjectWithMutationOperation
    {
        [Query]
        public int Method1()
        {
            return 0;
        }

        [Mutation]
        public int Method2()
        {
            return 0;
        }
    }
}