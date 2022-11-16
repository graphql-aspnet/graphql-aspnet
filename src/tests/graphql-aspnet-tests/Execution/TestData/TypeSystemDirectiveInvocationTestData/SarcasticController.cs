// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Execution.TypeSystemDirectiveInvocationTestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;

    public class SarcasticController : GraphController
    {
        [QueryRoot]
        public SarcasticObject RetrieveSarcasm()
        {
            var item = new SarcasticObject();
            item.Prop1 = "some sarcastic sentence";
            return item;
        }
    }
}