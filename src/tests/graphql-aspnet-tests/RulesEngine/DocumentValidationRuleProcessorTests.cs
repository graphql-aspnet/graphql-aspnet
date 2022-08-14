// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.ValidationRules
{
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.RulesEngine;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.ValidationRules.RuleCheckTestData;
    using NUnit.Framework;

    [TestFixture]
    public class DocumentValidationRuleProcessorTests
    {
        public static readonly List<object> TestQueries;

        private static void AddQuery(string ruleNumberToBreak, string query)
        {
            TestQueries.Add(new object[] { ruleNumberToBreak, query });
        }

        static DocumentValidationRuleProcessorTests()
        {
            TestQueries = new List<object>();

            // no such operation type as 'fakeOperationType'
            AddQuery("5.1.1", "fakeOperationType Operation1{ peopleMovers { elevator(id: 5){id, name} } }");

            // mutation is not valid in this schema
            AddQuery("5.2", "mutation Operation1{ peopleMovers { elevator(id: 5){id, name } } }");

            // duplciate named operations
            AddQuery("5.2.1.1", "query Operation1{ peopleMovers { elevator(id: 5){id, name } } }" +
                                "query Operation1{ peopleMovers { elevator(id: 8){id, name } } }");

            // Anonymous operation must be declared alone
            AddQuery("5.2.2.1", "{ peopleMovers { elevator(id: 5){id, name} } }" +
                                "query Operation1{ peopleMovers { elevator(id: 8){id, name } } }");

            AddQuery("5.2.2.1", "query { peopleMovers { elevator(id: 5){id, name} } }" +
                                "query { peopleMovers { elevator(id: 8){id, name } } }");

            // field "search" not declared on "query"
            AddQuery("5.3.1", "{ search { " +
                              "     id, name " +
                              "}}");

            // search returns a union. Unions cannot directly contain fields (other than __typename)
            AddQuery("5.3.1", "{ peopleMovers { search { " +
                              "     id " +
                              "}}}");

            // return names at same level must be identidical (different input values)
            AddQuery("5.3.2", "{ peopleMovers { " +
                              "     elevator(id: 5) { id, name }" +
                              "     elevator(id: 18) { id, name } " +
                              "}}");

            // return names at same level must be identical (aliased to same name but different return types)
            AddQuery("5.3.2", "{ peopleMovers { " +
                              "     elevator(id: 5) { id, name }" +
                              "     elevator: escalator(id: 5) { id, name } " +
                              "}}");

            // leaf fields must not have a child field set (name is a string, it has no fields)
            AddQuery("5.3.3", "query Operation1{ peopleMovers { elevator(id: 5){id, name { lastName } } } }");

            // non-leaf fields must have a child field set (elevator is an object)
            AddQuery("5.3.3", "query Operation1{ peopleMovers { elevator(id: 5) } }");

            // argument must be defined on the field in the schema  (notAnArg is not defined)
            // note: id is a required arg on the field (added to prevent a flag of rule 5.4.2.1 in this test)
            AddQuery("5.4.1", "query Operation1{ peopleMovers { elevator(id: 5, notAnArg: 5){ id name } } }");

            // argument must be defined on the directive in the schema (notAnArg is not defined)
            // note: someValue is a required arg on restrict (added to prevent a flag of rule 5.4.2.1 in this test)
            AddQuery("5.4.1", "query Operation1{ peopleMovers @restrict(notAnArg: 1, someValue: 1) { elevator(id: 5){ id name } } }");

            // arguments must be unique (field)
            AddQuery("5.4.2", "query Operation1{ peopleMovers { elevator(id: 5, id: 5){ id name } } }");

            // arguments must be unique (directive)
            AddQuery("5.4.2", "query Operation1{ peopleMovers @restrict(someValue: 1, someValue: 2) { elevator(id: 5){ id name } } }");

            // required argument must be provided (id is required on elevator but not provided)
            AddQuery("5.4.2.1", "query Operation1{ peopleMovers { elevator{ id name } } }");

            // required argument must be provided ("someValue" required on @Restrict but not provided)
            AddQuery("5.4.2.1", "query Operation1{ peopleMovers @restrict { elevator (id: 5) { id name } } }");

            // named fragments must be unique
            AddQuery("5.5.1.1", "query Operation1{ peopleMovers { elevator(id: 5){ ...frag1  } } } fragment frag1 on Elevator { id } fragment frag1 on Elevator { name} ");

            // named fragments must have a declared target type (no type on frag1)
            AddQuery("5.5.1.2", "query Operation1{ peopleMovers { elevator(id: 5){ ...frag1  } } } fragment frag1 { id }");

            // named fragments must have a declared target type (invalid type on frag1)
            AddQuery("5.5.1.2", "query Operation1{ peopleMovers { elevator(id: 5){ ...frag1  } } } fragment frag1 on Bob { id }");

            // inlined fragments must have a declared target type (invalid type bob)
            AddQuery("5.5.1.2", "query Operation1{ peopleMover(id: 5) { ... on Bob  { id } } } ");

            // all declared target types of a fragment must be Union, interface or object on named fragment
            // frag 1 declars a target of the string scalar
            AddQuery("5.5.1.3", "query Operation1{ peopleMovers { elevator(id: 5){ ...frag1  } } } fragment frag1 on String { unknownField1  }");

            // all declared target graph types must be Union, interface or object on inline fragment
            AddQuery("5.5.1.3", "query Operation1{ peopleMovers { elevator(id: 5){ ... on String { unknownField1 } } } } ");

            // all declared named fragments must be spread at least once
            AddQuery("5.5.1.4", "query Operation1{ peopleMovers { elevator(id: 5){ id name } } } fragment frag1 on Elevator { id }");

            // any spread named fragment must exist in the document
            AddQuery("5.5.2.1", "query Operation1{ peopleMovers { elevator(id: 5){ id ...frag1 } } }");

            // spreading a named fragment must not form a cycle
            AddQuery("5.5.2.2", "query Operation1{ peopleMovers { elevator(id: 5){ ...frag1 } } } fragment frag1 on Elevator { ...frag2 id name } fragment frag2 on Elevator { ...frag1 height} ");

            // spreading a fragment: Object inside object (objects must match)
            // (Elevator is not an Escalator)
            AddQuery("5.5.2.3.1", "query Operation1{ peopleMovers { elevator(id: 5){ ...frag1 } } } fragment frag1 on Escalator { id name }");

            // spreading an inline fragment: Object inside object (objects must match)
            // (Elevator is not an Escalator)
            AddQuery("5.5.2.3.1", "query Operation1{ peopleMovers { elevator(id: 5){ ... on Escalator {id} } } } ");

            // spreading a fragment: Abstract inside object (object must "be a part of" the abstract type)
            // (Elevator does not implement interface HoriztonalMover)
            AddQuery("5.5.2.3.2", "query Operation1{ peopleMovers { elevator(id: 5){ ...frag1 } } } fragment frag1 on HorizontalMover { id }");

            // spreading a inline fragment: Abstract inside object (object must "be a part of" the abstract type)
            // (Elevator does not implement interface HoriztonalMover)
            AddQuery("5.5.2.3.2", "query Operation1{ peopleMovers { elevator(id: 5){ ... on HorizontalMover { id } } } }");

            // spreading a fragment: object inside an abstract (abstract must "contain" the object )
            // (HoriztonalMover is not implemented by Elevator)
            AddQuery("5.5.2.3.3", "query Operation1{ peopleMovers { horizontalMover (id: \"5\"){ ...frag1 } } } fragment frag1 on Elevator { id name }");

            // spreading a inline fragment: object inside an abstract (abstract must "contain" the object )
            // (HoriztonalMover is not implemented by Elevator)
            AddQuery("5.5.2.3.3", "query Operation1{ peopleMovers { horizontalMover (id: \"5\"){ ...on Elevator { id name } } } }");

            // spreading a fragment: abstract inside an abstract (abstract must "contain" an intersection with other abstract)
            // (no object types implement both HorizontalMover and VerticalMover interfaces)
            AddQuery("5.5.2.3.4", "query Operation1{ peopleMovers { horizontalMover(id: \"5\"){ ...frag1 } } } fragment frag1 on VerticalMover { id }");

            // spreading a inline fragment: abstract inside an abstract (abstract must "contain" an intersection with other abstract)
            // (no object types implement both HorizontalMover and VerticalMover interfaces)
            AddQuery("5.5.2.3.4", "query Operation1{ peopleMovers { horizontalMover(id: \"5\"){ ...on VerticalMover { id } } } }");

            // required argument must be provided (matchElevator accepts in an input object of "Input_Elevator", simulate passing all possible non-object types: number, stirng, enum, bool)
            AddQuery("5.6.1", "query Operation1{ peopleMovers { matchElevator(e: 5) { id name } } }");
            AddQuery("5.6.1", "query Operation1{ peopleMovers { matchElevator(e: \"someString\") { id name } } }");
            AddQuery("5.6.1", "query Operation1{ peopleMovers { matchElevator(e: SOMEENUM) { id name } } }");

            // the supplied value of "someString" for input field "id" is not a number
            AddQuery("5.6.1", "query Operation1{ peopleMovers { elevatorsByBuilding(building: {id: \"someString\"}) { id name } } }");

            // INPUT_OBJECT: type must contain no unknown properties for the target graph type
            AddQuery("5.6.2", "query Operation1{ peopleMovers { matchElevator(e: {id: 5, nonExistantProp: 18}) { id name } } }");

            // INPUT_OBJECT:All properties on an input object must be unique
            AddQuery("5.6.3", "query Operation1{ peopleMovers { matchElevator(e: {id: 5, id: 7, name: \"South Elevator\"} ) { id name } } }");

            // INPUT_OBJECT:fields marked as "required" must be supplied  (id on field is marked required)
            AddQuery("5.6.4", "query Operation1{ peopleMovers { matchElevator(e: {name: \"South Elevator\"}) { id name } } }");

            // INPUT_OBJECT: checks nested input objects on an input object  (address has a required field of id)
            AddQuery("5.6.4", "query Operation1{ peopleMovers { elevatorsByBuilding(building: { id: 5, address: {name: \"shore line\"} }) { id name } } }");

            // directive must exist (no such directive as @NotARealThing)
            AddQuery("5.7.1", "query Operation1{ peopleMovers @notARealThing { elevator (id: 5) { id name } } }");

            // directive defined at appropriate locations (the restrict directive is not allowed at the "query" level)
            AddQuery("5.7.2", "query Operation1 @restrict(someValue: 10) { peopleMovers { elevator (id: 5) { id name } } }");

            // non-repeateable directive defined no more than once per location
            AddQuery("5.7.3", "query Operation1{ peopleMovers  @restrict(someValue: 10)  @restrict(someValue: 10) { elevator (id: 5) { id name } } }");

            // all variable names must be unique
            AddQuery("5.8.1", "query Operation1($var1: Int!, $var1: Int!){ peopleMovers { elevator (id: $var1) { id name } } }");

            // all variable must be of a valid type (HorizontalMover is an interface)
            AddQuery("5.8.2", "query Operation1($var1: HorizontalMover){ peopleMovers { elevator (id: $var1) { id name } } }");

            // all variable must be of a valid type (HorizontalMover is an interface)
            AddQuery("5.8.2", "query Operation1($var1: HorizontalMover){ peopleMovers { ...frag1 } } fragment frag1 on Query_PeopleMovers { elevator (id: $var1) { id name }  } ");

            // all used variables must be declared ($var2 is not declared)
            AddQuery("5.8.3", "query Operation1($var1: Int!){ peopleMovers { elevator (id: $var1) @restrict(someValue: $var2) { id name } } }");

            // all used variables must be declared ($var2 in referenced fragment is not declared by the operation)
            AddQuery("5.8.3", "query Operation1($var1: Int!){ peopleMovers { elevator (id: $var1) { id }  ...frag1 } } fragment frag1 on Query_PeopleMovers { ele: elevator(id:$var2) { id } }");

            // all variable must be used ($var2 is not referenced)
            AddQuery("5.8.4", "query Operation1($var1: Int!, $var2: String){ peopleMovers { elevator (id: $var1) { id name } } }");

            // all variable must be used ($var2 is not referenced through fragment)
            AddQuery("5.8.4", "query Operation1($var1: Int!, $var2: String){ peopleMovers { ...frag1 } } fragment frag1 on Query_PeopleMovers { elevator (id: $var1) { id name } } ");

            // all variable must be valid where used ($var1 is an int as required by elevator:id)
            AddQuery(
                "5.8.5",
                "query Operation1($var1: String!){ peopleMovers { elevator (id: $var1) { id name } } }");

            // all variable must be valid where used ($var1 is an int as required by elevator:id but
            // fragment recieves a string from its operation)
            AddQuery(
                "5.8.5",
                "query Operation1($var1: String!){ peopleMovers { ... frag1 } } fragment frag1 on Query_PeopleMovers {elevator (id: $var1) { id name } }");

            // all variables must be valid where used
            // $var1 is declared as a string but "horizontalMover" requires an "ID".
            // Special case where "GraphID" is declared as a string as well
            // but since type expressions don't match a failure must occur
            AddQuery(
                "5.8.5",
                "query Operation1($var1: String!){ peopleMovers { horizontalMover (id: $var1) { id } } }");

            // var1 is declared as an Int, id accepts an Int!, value is denied because default is null
            AddQuery(
                "5.8.5",
                "query Operation1($var1: Int = null){ peopleMover (id: $var1) { id } }");

            // var1 is declared as an Int, id accepts an Int!, value is denied because no default
            // for the variable is declared
            AddQuery(
                "5.8.5",
                "query Operation1($var1: Int){ peopleMover (id: $var1) { id } }");

            // var1 is declared as an String!, the list item type is Int!, value should be denied
            AddQuery(
                "5.8.5",
                "query Operation1($var1: String!){ escalators (ids: [$var1, 3]) { id } }");

            // var1 is declared as an Int with <null> default, verticalMove.id is declared
            // as Int!
            AddQuery(
                "5.8.5",
                "query Operation1($var1: Int = null){ peopleMovers { verticalMover (id: $var1) { id } } }");

            // var1 is declared as an Int with <null> default, e.id is declared
            // as Int!
            AddQuery(
                "5.8.5",
                "query Operation1($var1: Int = null){ peopleMovers { matchElevator (e: {id: $var1 name: \"bob\" }) { id } } }");
        }

        [TestCaseSource(nameof(TestQueries))]
        public void ExecuteRule_EnsureCorrectErrorIsGenerated(string expectedRuleError, string queryText)
        {
            var server = new TestServerBuilder()
                .AddType<PeopleMoverController>()
                .AddType<AllowDirective>()
                .AddType<RestrictDirective>()
                .Build();

            // parse the query
            var document = server.CreateDocument(queryText);

            // execute the document validation
            var validationContext = new DocumentValidationContext(server.Schema, document);
            var processor = new DocumentValidationRuleProcessor();
            processor.Execute(validationContext);

            // inspect for error codes
            if (document.Messages.Count != 1)
            {
                var errors = document.Messages.Where(
                    x => x.MetaData != null &&
                    x.MetaData.ContainsKey("Rule"))
                        .Select(x => x.MetaData["Rule"]);

                string errorMessage = $"Expected 1 error ({expectedRuleError}) but recieved {document.Messages.Count}: '{string.Join(", ", errors)}'";
                Assert.Fail(errorMessage);
            }

            var message = document.Messages[0];
            Assert.IsNotNull(message.MetaData);
            Assert.IsTrue(message.MetaData.ContainsKey("Rule"));

            var ruleBroke = message.MetaData["Rule"];
            Assert.AreEqual(expectedRuleError, ruleBroke.ToString());
        }
    }
}