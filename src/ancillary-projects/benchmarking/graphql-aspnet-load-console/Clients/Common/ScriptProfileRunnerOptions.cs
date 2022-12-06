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
    /// A set of configuration options to properly setup
    /// a script runner for a new run.
    /// </summary>
    public class ScriptProfileRunnerOptions
    {
        /// <summary>
        /// Gets the title of the overall run.
        /// </summary>
        /// <value>The title.</value>
        public string Title { get; init; }

        /// <summary>
        /// Gets the amount of time between react report refresh.
        /// </summary>
        /// <value>The report refresh delay.</value>
        public TimeSpan ReportRefreshDelay { get; init; }

        /// <summary>
        /// Gets the amount of jitter, in terms of milliseconds to allow the
        /// report refresh delay to vary by (Default: 0, no jitter).
        /// </summary>
        /// <value>The report fresh jitter.</value>
        public int ReportRefreshJitterMs { get; init; }

        /// <summary>
        /// Gets the amount of time to wait between starting individual client profiles.
        /// </summary>
        /// <value>The client profile cooldown.</value>
        public TimeSpan ClientProfileCooldown { get; init; }
    }
}