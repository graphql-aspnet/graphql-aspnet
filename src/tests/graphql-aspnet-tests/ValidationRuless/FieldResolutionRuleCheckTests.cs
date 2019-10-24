// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.ValidationRuless
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Interfaces.Response;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.ValidationRuless.RuleCheckTestData;
    using NUnit.Framework;

    [TestFixture]
    public class FieldResolutionRuleCheckTests
    {
        public static readonly List<object> TestQueries;

        private static void AddQuery(string ruleNumberToBreak, string query)
        {
            TestQueries.Add(new object[] { ruleNumberToBreak, query });
        }

        static FieldResolutionRuleCheckTests()
        {
            TestQueries = new List<object>();

            // specific object return type
            // -----------------------------
            // the field "notAnElevator" should return an Elevator according to the schema
            // definition but the method itself (the user code) returns an escalator
            // this should fail field completion
            AddQuery(
                "6.4.3",
                "query Operation1{ peopleMovers { notAnElevator{ id name } } }");

            // interface return type
            // -----------------------------
            // the field "notAHoriztonalMover" should return an object that implements IHorizontalMover according to the
            // schema but the user code generates and returns an elevator which does not implement the interface
            // this should fail field completion
            AddQuery(
                "6.4.3",
                "query Operation1{ peopleMovers { notAHoriztonalMover { id } } }");

            // union return type
            // -----------------------------
            // the field "notAUnionMember" should return an object that is part of the defined union (EscalatorOrBuilding)
            // but the user code generates and returns an elevator which is not defined on the union
            // this should fail field completion
            AddQuery(
                "6.4.3",
                "query Operation1{ peopleMovers { notAUnionMember { ... on Escalator { id name} ... on Building {id } } } }");

            // enum return type
            // ----------------------------
            // the return type for ExtendedElevator.Type is defined in C# code as an enum in the schema.
            // The property in code, however; uses a casted integer value of "3" as the value for the property.
            // since "3" is not a defined enum value it fails the result even if .NET can work with it.
            AddQuery("6.4.3", "query Operation1{ peopleMovers { extendedElevator(id: 5){ type } } }");

            // resolver returns a null object when not allowed
            // -------------------------------
            // requiredElevator is defined to not allow a null value (type expression: Elevator! )
            // however the controller method returns null as its result
            // this fails rule 6.4.3
            AddQuery("6.4.3", "query Operation1{ peopleMovers { requiredElevator{ id name } } }");

            // resolver does not return a list when expected
            // -------------------------------
            // allElevators is defined to be a list of elevators (type expression:  [Elevator] )
            // however the controller method returns a single elevator
            // this fails rule 6.4.3
            AddQuery("6.4.3", "query Operation1{ peopleMovers { allElevators{ id name } } }");
        }

        [TestCaseSource(nameof(TestQueries))]
        public async Task Rule_6_4_3_EnsureCorrectErrorIsGenerated(string expectedRuleError, string queryText)
        {
            var server = new TestServerBuilder()
                .AddGraphType<PeopleMoverController>()
                .AddGraphType<AllowDirective>()
                .AddGraphType<RestrictDirective>()
                .Build();

            var queryBuilder = server.CreateQueryContextBuilder();
            queryBuilder.AddQueryText(queryText);

            // execute the query
            var result = await server.ExecuteQuery(queryBuilder);

            // inspect for error codes
            if (result.Messages.Count != 1)
            {
                var errors = result.Messages.Where(
                    x => x.MetaData?.ContainsKey("Rule") == true)
                        .Select(x => x.MetaData["Rule"]);

                string errorMessage = $"Expected 1 error ({expectedRuleError}) but recieved {result.Messages.Count}: '{string.Join(", ", errors)}'";
                Assert.Fail(errorMessage);
            }

            var message = result.Messages[0];
            Assert.IsNotNull(message.MetaData);
            Assert.IsTrue(message.MetaData.ContainsKey("Rule"));

            var ruleBroke = message.MetaData["Rule"];
            Assert.AreEqual(expectedRuleError, ruleBroke.ToString());
        }

        [Test]
        public async Task Rule_6_4_4_ErrorPropegation_InvalidatesParentObjects()
        {
            var server = new TestServerBuilder()
                .AddGraphType<PeopleMoverController>()
                .AddGraphType<AllowDirective>()
                .AddGraphType<RestrictDirective>()
                .Build();

            // allElevatorsWithANull is defined to be a list of elevators (type expression:  [Elevator!])
            // it returns a list but with the second item (of 3) having a null name that when resolved
            // fails validation for that individual list item as each elevator must have a name according
            // the the Elevator's individual type requirements.
            // This results in the second list item becoming null...which in turn results in the
            // list being invalid (since it now contains a null element) this fails rule 6.4.4
            //
            // ensure that this single invalid name in list item 2 is propegated to null out
            // the whole list
            var queryBuilder = server.CreateQueryContextBuilder();
            queryBuilder.AddQueryText("query Operation1{ peopleMovers { allElevatorsWithANull{ id name } } }");

            var result = await server.ExecuteQuery(queryBuilder);

            // ensure error 6.4.3 is recorded (and that its all thats recorded)
            Assert.AreEqual(1, result.Messages.Count);
            var message = result.Messages[0];
            Assert.IsNotNull(message.MetaData);
            Assert.IsTrue(message.MetaData.ContainsKey("Rule"));

            var ruleBroke = message.MetaData["Rule"];
            Assert.AreEqual("6.4.3", ruleBroke.ToString());

            // since only one top level query was made (peopleMovers)
            // this reslts in all fields of the query being null, forcing "data" to also be null
            Assert.IsNull(result.Data);
        }

        [Test]
        public async Task Rule_6_4_4_ErrorPropegation_InvalidatesParentObjects_MultiRootFields()
        {
            var server = new TestServerBuilder()
                .AddGraphType<PeopleMoverController>()
                .AddGraphType<AllowDirective>()
                .AddGraphType<RestrictDirective>()
                .Build();

            // field "invalidMovers" will result in a null field because of an error caused by
            // down stream field peopleMovers.allElevatorsWithANull[1].Name
            //
            // however the field "validMovers" should resolve just fine
            // resulting in a data set with invalidMovers as null but with validMovers as having a value
            // fully populated
            var queryBuilder = server.CreateQueryContextBuilder();
            queryBuilder.AddQueryText(@"query Operation1{
                                    invalidMovers: peopleMovers { allElevatorsWithANull{ id name } }
                                    validMovers: peopleMovers { elevator(id: 5) { id name } }
                                }");

            var result = await server.ExecuteQuery(queryBuilder);

            // ensure error 6.4.3 is recorded (and that its all thats recorded)
            Assert.AreEqual(1, result.Messages.Count);
            var message = result.Messages[0];
            Assert.IsNotNull(message.MetaData);
            Assert.IsTrue(message.MetaData.ContainsKey("Rule"));

            var ruleBroke = message.MetaData["Rule"];
            Assert.AreEqual("6.4.3", ruleBroke.ToString());

            Assert.IsNotNull(result.Data);
            var data = result.Data as IResponseFieldSet;
            Assert.IsNull(data.Fields["invalidMovers"]);
            Assert.IsNotNull(data.Fields["validMovers"]);
        }
    }
}