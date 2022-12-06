// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.Execution.RulesEngine.RuleCheckTestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Schemas.TypeSystem;

    [GraphType(PreventAutoInclusion = true)]
    public class ElevatorBindingModel
    {
        public int Id { get; set; }

        [GraphField("name", TypeExpression = "Type!")]
        public string Name { get; set; }
    }
}