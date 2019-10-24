// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.ValidationRuless.RuleCheckTestData
{
    public class ExtendedElevator : IPeopleMover, IVerticalMover
    {
        public ExtendedElevator(int id, string name)
        {
            this.Id = id;
            this.Name = name;
            this.Type = (ElevatorType)3;
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public ElevatorType Type { get; set; }
    }
}