// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Execution.Metrics
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    /// <summary>
    /// A single entry of related metrics data tied to the Apollo Tracing metrics standard.
    /// </summary>
    public class ApolloMetricsEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApolloMetricsEntry" /> class.
        /// </summary>
        public ApolloMetricsEntry()
        {
            this.MetaData = new Dictionary<string, object>();
        }

        /// <summary>
        /// Gets or sets the number of ticks from the start time that this entry "began".
        /// </summary>
        /// <value>The start offset.</value>
        public long StartOffsetTicks { get; set; }

        /// <summary>
        /// Gets the start offset in nanoseconds.
        /// </summary>
        /// <value>The start offset nanoseconds.</value>
        public long StartOffsetNanoseconds => TicksToNanoSeconds(this.StartOffsetTicks);

        /// <summary>
        /// Gets or sets the number of ticks from the start time that this entry "ended".
        /// </summary>
        /// <value>The end offset.</value>
        public long EndOffsetTicks { get; set; }

        /// <summary>
        /// Gets any additiona key/value pair meta data related to this entry.
        /// </summary>
        /// <value>The meta data.</value>
        public IDictionary<string, object> MetaData { get; }

        /// <summary>
        /// Gets the duration of the entry in ticks.
        /// </summary>
        /// <value>The duration.</value>
        public long Duration => this.EndOffsetTicks > this.StartOffsetTicks ? this.EndOffsetTicks - this.StartOffsetTicks : 0;

        /// <summary>
        /// Gets the duration of this entry in nanoseconds.
        /// </summary>
        /// <value>The duration nano seconds.</value>
        public long DurationNanoSeconds => TicksToNanoSeconds(this.Duration);

        /// <summary>
        /// Converts Stopwatch ticks into nanoseconds.
        /// </summary>
        /// <param name="ticks">The ticks.</param>
        /// <returns>System.Double.</returns>
        protected static long TicksToNanoSeconds(long ticks)
        {
            if (ticks <= 0)
                return 0;

            return Convert.ToInt64(((double)ticks / Stopwatch.Frequency) * 1000000000);
        }
    }
}