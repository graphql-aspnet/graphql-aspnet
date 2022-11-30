// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.SubscriberLoadTest.TestConsole.Clients.Common
{
    using System;

    /// <summary>
    /// A set of options controlling the overall script execution.
    /// </summary>
    public class ScriptProfileClientOptions
    {
        /// <summary>
        /// Gets the title of this client options setup.
        /// </summary>
        /// <value>The title.</value>
        public string Title { get; init; }

        /// <summary>
        /// Gets a function that accepts a global script number and can generate a new script instance when needded.
        /// </summary>
        /// <value>The script maker.</value>
        public ScriptMakerDelgate ScriptMaker { get; init; }

        /// <summary>
        /// Gets the total number of "users" to mimic. A client represents
        /// a single Http Client connecting out to the target server.
        /// </summary>
        /// <value>The total scripts.</value>
        public int TotalClientInstances { get; init; }

        /// <summary>
        /// Gets the amount of time to wait before generating and executing clients.
        /// </summary>
        /// <value>The startup delay.</value>
        public TimeSpan StartupDelay { get; init; }

        /// <summary>
        /// Gets the amount of time to way between client instance creations.
        /// </summary>
        /// <value>The script creation cooldown.</value>
        public TimeSpan ClientCreationCooldown { get; init; }

        /// <summary>
        /// Gets the number of times each client should execute the test. These tests
        /// are executed in parallel.
        /// </summary>
        /// <remarks>
        /// To many iterations can lead to port exaustion on part of the single client.
        /// </remarks>
        /// <value>The iterations per script.</value>
        public int IterationsPerClient { get; init; }

        /// <summary>
        /// Gets the total number of calls to complete per iterations.
        /// </summary>
        /// <value>The total calls per iteration.</value>
        public int CallsPerIteration { get; init; }

        /// <summary>
        /// Gets the amount of time to wait between starting a new
        /// iteration within a single script.
        /// </summary>
        /// <value>The script iteration cooldown.</value>
        public TimeSpan IterationCooldown { get; init; }

        /// <summary>
        /// Gets the minimum amount of time, in milliseconds, to wait
        /// between each call within a single script iteration.
        /// </summary>
        /// <value>The minimum delay between iteration calls in milliseconds.</value>
        public int MinDelayBetweenCallsMs { get; init; }

        /// <summary>
        /// Gets the maximum amount of time, in milliseconds, to wait
        /// between each call within a single script iteration.
        /// </summary>
        /// <value>The maximum delay between iteration calls in milliseconds.</value>
        public int MaxDelayBetweenCallsMs { get; init; }
    }
}