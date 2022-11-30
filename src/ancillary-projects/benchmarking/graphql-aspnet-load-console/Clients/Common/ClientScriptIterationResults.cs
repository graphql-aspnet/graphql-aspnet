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

    /// <summary>
    /// A contained set of results for a given query.
    /// </summary>
    public class ClientScriptIterationResults
    {
        private Dictionary<string, int> _callsByCode;
        private List<string> _errors;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientScriptIterationResults" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="expectedCalls">The expected number of calls to be made.</param>
        /// <param name="resultType">Type of the results recorded by this instance.</param>
        public ClientScriptIterationResults(string id, int expectedCalls, ClientScriptResultType resultType)
        {
            _callsByCode = new Dictionary<string, int>();
            _errors = new List<string>();
            this.Id = id;
            this.ExpectedCalls = expectedCalls;
            this.ResultType = resultType;
        }

        /// <summary>
        /// Adds the response code to the counted set.
        /// </summary>
        /// <param name="statusCode">The status code.</param>
        /// <param name="error">The error message returned on the call, if any.</param>
        public void AddResultsByStatusCode(int statusCode, string error = null)
        {
            this.AddResults(statusCode.ToString(), error);
        }

        /// <summary>
        /// Adds a completed call under the given category.
        /// </summary>
        /// <param name="category">The category to file the result under.</param>
        /// <param name="error">The error message returned on the call, if any.</param>
        public void AddResults(string category, string error = null)
        {
            lock (_callsByCode)
            {
                if (!_callsByCode.ContainsKey(category))
                    _callsByCode.Add(category, 0);

                _callsByCode[category]++;
                this.CompletedCalls += 1;

                if (!string.IsNullOrWhiteSpace(error))
                {
                    _errors.Add($"Call {this.CompletedCalls - 1}: {error}");
                }
            }
        }

        /// <summary>
        /// Gets the total number of calls may by this client.
        /// </summary>
        /// <value>The total calls.</value>
        public int CompletedCalls { get; private set; }

        /// <summary>
        /// Gets the number of calls expected to be completed by this client.
        /// </summary>
        /// <value>The expected number of calls.</value>
        public int ExpectedCalls { get; }

        /// <summary>
        /// Gets a set of any errors raised by call execution.
        /// </summary>
        /// <value>The errors.</value>
        public IReadOnlyList<string> Errors => _errors;

        /// <summary>
        /// Gets the count of return status codes for the calls in this client.
        /// </summary>
        /// <value>The count by status code.</value>
        public IReadOnlyDictionary<string, int> CountByCategory => _callsByCode;

        /// <summary>
        /// Gets the identifier of this result capture.
        /// </summary>
        /// <value>The identifier.</value>
        public string Id { get; }

        /// <summary>
        /// Gets the type of results recorded by this instance.
        /// </summary>
        /// <value>The type of the result.</value>
        public ClientScriptResultType ResultType { get; }
    }
}