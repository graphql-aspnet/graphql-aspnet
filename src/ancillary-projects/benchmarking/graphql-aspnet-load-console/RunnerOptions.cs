// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.SubscriberLoadTest.TestConsole
{
    using System;
    using GraphQL.AspNet.SubscriberLoadTest.TestConsole.Clients.Common;
    using GraphQL.AspNet.SubscriberLoadTest.TestConsole.Clients.GraphQLSamples;
    using GraphQL.AspNet.SubscriberLoadTest.TestConsole.Clients.RestSamples;

    /// <summary>
    /// A collection of various commonly used script components for configuring a
    /// test run.
    /// </summary>
    internal class RunnerOptions
    {
        /// <summary>
        /// A runner configuration object used to setup the script runner
        /// and reporting parameters.
        /// </summary>
        public static ScriptProfileRunnerOptions ProfileSession = new ()
        {
            Title = "Profile Session",

            ReportRefreshDelay = TimeSpan.FromMilliseconds(1000),
            ReportRefreshJitterMs = 300,
        };

        /// <summary>
        /// A test run that will execute a REST request to fetch
        /// a single Donut object from the server.
        /// </summary>
        public static ScriptProfileClientOptions RestQuery = new ()
        {
            Title = "GET Donut",

            ScriptMaker = (scriptNumber) => new RetrieveDonutRestQuery(
                Constants.REST_URL_BASE,
                scriptNumber),

            StartupDelay = TimeSpan.FromSeconds(2),
            TotalClientInstances = 25,
            ClientCreationCooldown = TimeSpan.FromMilliseconds(45),

            IterationsPerClient = 10,
            IterationCooldown = TimeSpan.FromMilliseconds(300),

            CallsPerIteration = 200000,
            MinDelayBetweenCallsMs = 5,
            MaxDelayBetweenCallsMs = 25,
        };

        /// <summary>
        /// A test run to execute a GraphQL Query request to fetch a signle
        /// Donut object from the server.
        /// </summary>
        public static ScriptProfileClientOptions GraphQLQuery = new ()
        {
            Title = "Query Donut",

            ScriptMaker = (scriptNumber) => new RetrieveDonutGraphQLQuery(
                Constants.GRAPHQL_URL,
                scriptNumber),

            StartupDelay = TimeSpan.FromSeconds(2),
            TotalClientInstances = 10,
            ClientCreationCooldown = TimeSpan.FromMilliseconds(45),
            IterationsPerClient = 2,
            IterationCooldown = TimeSpan.FromMilliseconds(300),

            CallsPerIteration = 200000,
            MinDelayBetweenCallsMs = 5,
            MaxDelayBetweenCallsMs = 15,
        };

        /// <summary>
        /// A test run to execute a GraphQL Mutation request to modify and retrieve
        /// a signle Donut object from the server. This exeuction also
        /// triggers a subscription event to be fired against the mutated object.
        /// </summary>
        public static ScriptProfileClientOptions GraphQLMutation = new()
        {
            Title = "Mutate Donut",

            ScriptMaker = (scriptNumber) => new AddUpdateDonutMutation(
                Constants.GRAPHQL_URL,
                scriptNumber),

            TotalClientInstances = 10,
            ClientCreationCooldown = TimeSpan.FromMilliseconds(45),

            IterationsPerClient = 3,
            IterationCooldown = TimeSpan.FromMilliseconds(300),

            CallsPerIteration = 20000,
            MinDelayBetweenCallsMs = 5,
            MaxDelayBetweenCallsMs = 25,

            StartupDelay = TimeSpan.FromSeconds(5),
        };

        /// <summary>
        /// A test run that sets up a subscription client and activates a
        /// subscription on the test server to listen for events triggered
        /// by the <see cref="GraphQLMutation"/> script.
        /// </summary>
        public static ScriptProfileClientOptions GraphQlSubscription = new ()
        {
            Title = "Subscribe Donut",

            ScriptMaker = (scriptNumber) => new OnDonutUpdatedSubscription(
                Constants.GRAPHQL_URL,
                scriptNumber),

            TotalClientInstances = 15,
            ClientCreationCooldown = TimeSpan.FromMilliseconds(15),

            // should always be 1 for this script
            IterationsPerClient = 1,
            IterationCooldown = TimeSpan.FromMilliseconds(300),

            // listen for 1million events before closing
            CallsPerIteration = 1_000_000_000,

            StartupDelay = TimeSpan.FromSeconds(1),
        };
    }
}