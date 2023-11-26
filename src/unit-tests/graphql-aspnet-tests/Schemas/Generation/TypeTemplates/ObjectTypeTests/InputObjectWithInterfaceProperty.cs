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
    public class InputObjectWithInterfaceProperty
    {
        public int Id { get; set; }

        public IInputObject UnionProxy { get; set; }
    }
}