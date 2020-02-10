// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.ValidationRuless.RuleCheckTestData
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Schemas.TypeSystem;

    [GraphRoute("peopleMovers")]
    public class PeopleMoverController : GraphController
    {
        [SubscriptionRoot("elevatorMoved")]
        public Elevator RetrieveElevator(Elevator elevator, int id)
        {
            if (elevator.Id == id)
                return elevator;
            else
                return null;
        }

        [SubscriptionRoot("anyElevatorMoved")]
        public Elevator RetrieveElevator(Elevator elevator)
        {
            return elevator;
        }
    }
}