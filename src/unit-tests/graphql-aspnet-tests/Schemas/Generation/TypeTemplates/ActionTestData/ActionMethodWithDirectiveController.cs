// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Schemas.Generation.TypeTemplates.ActionTestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Tests.Schemas.Generation.TypeTemplates.DirectiveTestData;

    public class ActionMethodWithDirectiveController : GraphController
    {
        [Query]
        [ApplyDirective(typeof(DirectiveWithArgs), 202, "controller action arg")]
        public int Execute()
        {
            return 0;
        }
    }
}