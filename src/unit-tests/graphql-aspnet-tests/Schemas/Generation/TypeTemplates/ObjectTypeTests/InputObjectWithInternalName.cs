// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Schemas.Generation.TypeTemplates.ObjectTypeTests
{
    using System.Threading.Tasks;
    using GraphQL.AspNet.Attributes;

    [GraphType(InternalName = "InputObjectWithName_33")]
    public class InputObjectWithInternalName
    {
        public int Id { get; set; }
    }
}