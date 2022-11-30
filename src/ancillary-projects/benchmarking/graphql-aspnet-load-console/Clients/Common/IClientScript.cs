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
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// A profile of a query to execute within the runner.
    /// </summary>
    public interface IClientScript
    {
        /// <summary>
        /// Executes the client script.
        /// </summary>
        /// <param name="options">The options to use to govern the execution
        /// of the script.</param>
        /// <param name="cancelToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Task.</returns>
        Task ExecuteClientScript(ScriptProfileClientOptions options, CancellationToken cancelToken = default);

        /// <summary>
        /// Gets the client identifier assigned to this instance.
        /// </summary>
        /// <value>The client identifier.</value>
        string ClientId { get; }

        /// <summary>
        /// Gets the execution category for each of the iterations
        /// completed by this script.
        /// </summary>
        /// <value>The execution category.</value>
        string ExecutionCategory { get; }

        /// <summary>
        /// Gets the gathered results from each iteration executed via this client.
        /// </summary>
        /// <value>The results.</value>
        IReadOnlyList<ClientScriptIterationResults> Results { get; }
    }
}