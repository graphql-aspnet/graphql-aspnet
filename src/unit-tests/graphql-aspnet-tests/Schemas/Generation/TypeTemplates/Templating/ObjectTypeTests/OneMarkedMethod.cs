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

    public class OneMarkedMethod
    {
        public string Method1()
        {
            return string.Empty;
        }

        [GraphField]
        public string Method2(string arg1)
        {
            return string.Empty;
        }
    }
}