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

    public class ObjectThatInheritsExplicitMethodField : ObjectWithExplicitMethodField
    {
        [GraphField]
        public int FieldOnObject(int param2)
        {
            return 0;
        }
    }
}