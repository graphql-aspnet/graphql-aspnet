// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.Resolvers.ValueResolversTestData
{
    public struct ResolverStructA
    {
        public ResolverStructA(string prop1)
        {
            this.Prop1 = prop1;
        }

        public string Prop1 { get; }
    }
}