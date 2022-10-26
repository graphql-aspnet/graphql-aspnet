// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.ValidationRules
{
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.RulesEngine;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.Subscriptions.Tests.Mocks;
    using GraphQL.Subscriptions.Tests.ValidationRules.RuleCheckTestData;
    using NUnit.Framework;

    [TestFixture]
    public class DocumentGenerationRuleCheckTests
    {
        public static readonly List<object> TestQueries;

        private static void AddQuery(string ruleNumberToBreak, string query)
        {
            TestQueries.Add(new object[] { ruleNumberToBreak, query });
        }

        static DocumentGenerationRuleCheckTests()
        {
            TestQueries = new List<object>();

            // two top level fields in a subscription operation results in an error
            AddQuery("5.2.3.1", "subscription {  elevatorMoved(id: 5) { id, name }   anyElevatorMoved { id, name }     } ");

            // one top level field, but the hierarchy splits before a non-virtual field
            // is encountered by itself at a level of the hierarchy (elevatorNested1, elevatorNested2)
            AddQuery(
                "5.2.3.1.1",
                @"subscription {
                            peopleMovers {
                                elevatorNested1(id: 5) { id, name }
                                elevators {
                                    elevatorNested2(id: 15) { id, name }
                                }
                            }
                }");
        }

        [TestCaseSource(nameof(TestQueries))]
        public void ExecuteRule_EnsureCorrectErrorIsGenerated(string expectedRuleError, string queryText)
        {
            using var restorePoint = new GraphQLGlobalRestorePoint();

            var server = new TestServerBuilder()
                .AddType<PeopleMoverController>()
                .AddType<AllowDirective>()
                .AddType<RestrictDirective>()
                .AddSubscriptionServer()
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