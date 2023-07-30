// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************


namespace GraphQL.AspNet.Tests.Schemas.Generation.TypeTemplates.ControllerTestData
{
    using GraphQL.AspNet.Attributes;

    public class ControllerWithInheritedAction : BaseControllerWithAction
    {
        [Query]
        public int ChildControllerAction()
        {
            return 0;
        }
    }
}