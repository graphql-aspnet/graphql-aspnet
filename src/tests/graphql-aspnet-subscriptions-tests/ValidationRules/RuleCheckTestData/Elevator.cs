// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.ValidationRules.RuleCheckTestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Schemas.TypeSystem;

    public class Elevator : IPeopleMover, IVerticalMover
    {
        public Elevator(int id, string name)
        {
            this.Id = id;
            this.Name = name;
        }

        public int Id { get; set; }

        [GraphField("name", TypeExpression = TypeExpressions.IsNotNull)]
        public string Name { get; set; }
    }
}