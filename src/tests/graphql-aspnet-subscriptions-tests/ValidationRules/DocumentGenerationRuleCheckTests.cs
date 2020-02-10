﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.ValidationRuless
{
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.Subscriptions.Tests.TestServerHelpers;
    using GraphQL.Subscriptions.Tests.ValidationRuless.RuleCheckTestData;
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
        }

        [TestCaseSource(nameof(TestQueries))]
        public void ExecuteRule_EnsureCorrectErrorIsGenerated(string expectedRuleError, string queryText)
        {
            using var restorePoint = new GraphQLProviderRestorePoint();

            var server = new TestServerBuilder()
                .AddGraphType<PeopleMoverController>()
                .AddGraphType<AllowDirective>()
                .AddGraphType<RestrictDirective>()
                .AddSubscriptions()
                .Build();

            // parse the query
            var document = server.CreateDocument(queryText);

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