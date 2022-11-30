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
    using System.Threading.Tasks;

    public class InputObjectIWithTaskProperty
    {
        public int Id { get; set; }

        public Task<int> TaskProperty { get; set; }
    }
}