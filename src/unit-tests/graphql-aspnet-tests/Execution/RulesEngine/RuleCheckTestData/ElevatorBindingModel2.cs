// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.RulesEngine.RuleCheckTestData
{
    using System.ComponentModel.DataAnnotations;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Schemas.TypeSystem;

    [GraphType(PreventAutoInclusion = true)]
    public class ElevatorBindingModel2
    {
        public ElevatorBindingModel2()
        {
            this.Id = 35;
            this.Name = string.Empty;
        }

        public int Id { get; set; }

        [GraphField("name", TypeExpression = "Type!")]
        public string Name { get; set; }
    }
}