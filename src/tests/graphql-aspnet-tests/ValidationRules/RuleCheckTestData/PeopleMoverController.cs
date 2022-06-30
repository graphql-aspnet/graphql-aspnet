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
    using System.Collections.Generic;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Schemas.TypeSystem;

    [GraphRoute("peopleMovers")]
    public class PeopleMoverController : GraphController
    {
        [Query("elevator")]
        public Elevator RetrieveElevator(int id)
        {
            return new Elevator(id, "Some Vator");
        }

        [Query("requiredElevator", TypeExpression = TypeExpressions.IsNotNull)]
        public Elevator RetrieveRequiredElevator()
        {
            return null;
        }

        [Query("allElevators", typeof(IEnumerable<Elevator>))]
        public IGraphActionResult RetrieveAllElevator()
        {
            return this.Ok(new Elevator(5, "Some Vator"));
        }

        [Query("allElevatorsWithANull", typeof(IEnumerable<Elevator>), TypeExpression = TypeExpressions.IsList | TypeExpressions.IsNotNull)]
        public IGraphActionResult RetrieveAllElevatorsWithNullItem()
        {
            var list = new List<Elevator>();
            list.Add(new Elevator(4, "Vator 4"));
            list.Add(new Elevator(5, null));
            list.Add(new Elevator(6, "Vator 6"));

            return this.Ok(list);
        }

        [Query("extendedElevator")]
        public ExtendedElevator RetrieveExtendedElevator(int id)
        {
            return new ExtendedElevator(id, "Some Vator");
        }

        [Query("matchElevator")]
        public Elevator MatchElevator(ElevatorBindingModel e)
        {
            return null;
        }

        [Query("escalator")]
        public Escalator RetrieveEscalator(int id)
        {
            return new Escalator(id, "Some Scalator");
        }

        [QueryRoot("peopleMover")]
        public IPeopleMover RetrieveSingleMover(int id)
        {
            return null;
        }

        [Query("horizontalMover")]
        public IHorizontalMover RetrieveHoriztonalMover(GraphId id)
        {
            return null;
        }

        [Query("verticalMover")]
        public IVerticalMover RetrieveVerticalMover(int id)
        {
            return null;
        }

        [Query("elevatorsByBuilding")]
        public List<IPeopleMover> FetchAllElevators(Building building)
        {
            return null;
        }

        [Query("search", "ElevatorOrEscalator", typeof(Escalator), typeof(Elevator), TypeExpression = TypeExpressions.IsNotNullList)]
        public IGraphActionResult SearchPeopleMovers(string name = "*")
        {
            return null;
        }

        [Query("notAnElevator", typeof(Elevator))]
        public IGraphActionResult ExpectedElevatorButReturnsEscalator()
        {
            return this.Ok(new Escalator(5, "Escalator1"));
        }

        [Query("notAHoriztonalMover", typeof(IHorizontalMover))]
        public IGraphActionResult ExpectedHorizontalMoverButReturnsVerticalMover()
        {
            return this.Ok(new Elevator(5, "Elevator1"));
        }

        [Query("notAUnionMember", "EscalatorOrBuilding", typeof(Escalator), typeof(Building))]
        public IGraphActionResult ExpectedAUnionMemberTypeButReturnedAnElevator()
        {
            return this.Ok(new Elevator(5, "Elevator1"));
        }
    }
}