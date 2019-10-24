// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Execution
{
    using GraphQL.AspNet.Interfaces.Response;
    using GraphQL.AspNet.Middleware.FieldExecution;

    /// <summary>
    /// An interface capable of recieving events related to datapoints that can be tracked
    /// to produce a collection of metrics describing a query's execution path and timing information. The
    /// class implementing this interface MUST BE thread-safe as multiple resolvers will be started simultaniously.
    /// </summary>
    public interface IGraphQueryExecutionMetrics
    {
        /// <summary>
        /// The first method called when the query first begins processing. This method
        /// is called prior to the start of any execution phase.
        /// </summary>
        void Start();

        /// <summary>
        /// The last method called when a query is finished processing. This method is the last
        /// called after the final execution phase is complete.
        /// </summary>
        void End();

        /// <summary>
        /// Marks the start of the given phase. THe metrics package should make note of this to determine
        /// total duration of the phase if desired.
        /// </summary>
        /// <param name="phase">The phase to begin.</param>
        void StartPhase(string phase);

        /// <summary>
        /// Marks the end of the given phase. THe metrics package should make note of this to determine
        /// total duration of the phase if desired.
        /// </summary>
        /// <param name="phase">The phase to terminate.</param>
        void EndPhase(string phase);

        /// <summary>
        /// Marks the start of a single resolver attempting to generate data according to its own specifications.
        /// </summary>
        /// <param name="context">The context outlining the resolution that is taking place.</param>
        void BeginFieldResolution(GraphFieldExecutionContext context);

        /// <summary>
        /// Marks the end of a single resolver attempting to generate data according to its own specifications.
        /// </summary>
        /// <param name="context">The context outlining the resolution that is taking place.</param>
        void EndFieldResolution(GraphFieldExecutionContext context);

        /// <summary>
        /// Formats the output of the metrics into a set of nested key/value pairs that can be written to a graphql
        /// data response. The keys returned will be written directly to the "extensions" section of the graphql response.
        /// It is recommended to return a single toplevel key containing the entirety of your metrics data.
        /// </summary>
        /// <returns>IResponseFieldSet.</returns>
        IResponseFieldSet GenerateResult();
    }
}