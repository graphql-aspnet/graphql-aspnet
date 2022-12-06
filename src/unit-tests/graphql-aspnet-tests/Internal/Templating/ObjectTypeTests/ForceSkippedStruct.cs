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

    [GraphSkip]
    public struct ForceSkippedStruct
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }
}