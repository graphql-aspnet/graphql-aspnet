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
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;

    [GraphRoute("peopleMovers")]
    public class PeopleMoverController : GraphController
    {
        [SubscriptionRoot("elevatorMoved", EventName = "SINGLE_ELEVATOR_MOVED")]
        public Elevator RetrieveElevator(Elevator elevator, int id)
        {
            if (elevator.Id == id)
                return elevator;
            else
                return null;
        }

        [SubscriptionRoot("anyElevatorMoved", EventName ="ANY_ELEVATOR_MOVED")]
        public Elevator RetrieveElevator(Elevator elevator)
        {
            return elevator;
        }

        [Subscription("elevatorNested1")]
        public Elevator RetrieveNestedElevator(Elevator elevator, int id)
        {
            if (elevator.Id == id)
                return elevator;
            else
                return null;
        }

        [Subscription("/elevators/elevatorNested2")]
        public Elevator RetrieveDeepNestedElevator(Elevator elevator, int id)
        {
            if (elevator.Id == id)
                return elevator;
            else
                return null;
        }
    }
}