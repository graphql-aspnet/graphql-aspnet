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
    public class ElevatorBindingModel
    {
        public ElevatorBindingModel()
        {
            this.Name = string.Empty;
        }

        [Required]
        public int Id { get; set; }

        [GraphField("name", TypeExpression = "Type!")]
        public string Name { get; set; }
    }
}