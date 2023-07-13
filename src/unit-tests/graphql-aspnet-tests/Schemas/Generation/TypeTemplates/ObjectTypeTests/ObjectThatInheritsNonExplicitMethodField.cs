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
    public class ObjectThatInheritsNonExplicitMethodField : ObjectWithNonExplicitMethodField
    {
        public int FieldOnObject(int param2)
        {
            return 0;
        }
    }
}