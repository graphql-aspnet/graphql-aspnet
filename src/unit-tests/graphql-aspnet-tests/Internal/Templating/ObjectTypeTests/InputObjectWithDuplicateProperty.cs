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

    public class InputObjectWithDuplicateProperty
    {
        public int Id { get; set; }

        [GraphField("Id")]
        public int OtherId { get; set; }
    }
}