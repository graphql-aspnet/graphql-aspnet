// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.SubscriberLoadTest.TestConsole.Clients.RestSamples;

using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GraphQL.AspNet.SubscriberLoadTest.TestConsole.Clients.Common;

/// <summary>
/// A client script to execute a single mutation or query against the graphql server.
/// </summary>
public abstract class RestScriptBase : ClientScriptBase
{
    /// <summary>
    /// A common set of serializer configuration options.
    /// </summary>
    protected static readonly JsonSerializerOptions JSON_OPTIONS = new JsonSerializerOptions()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
    };

    private bool _isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="RestScriptBase" /> class.
    /// </summary>
    /// <param name="scriptNumber">The overall script number assigned to this instance.</param>
    /// <param name="executionCatgory">The execution catgory this script falls under.</param>
    /// <param name="categorySuffix">The category suffix to apply to this individual script.</param>
    protected RestScriptBase(
        int scriptNumber,
        string executionCatgory,
        string categorySuffix = "")
        : base(scriptNumber, executionCatgory, categorySuffix)
    {
        this.HttpClient = new HttpClient();
    }

    /// <inheritdoc />
    protected override async Task ExecuteSingleQuery(ClientScriptIterationResults recordTo, CancellationToken cancelToken = default)
    {
        try
        {
            await this.ExecuteSingleRestQuery(recordTo, cancelToken);
        }
        catch (Exception ex)
        {
            this.RecordException(recordTo, ex);
        }
    }

    /// <summary>
    /// When overriden in a child class, executes a single rest query and records
    /// the results.
    /// </summary>
    /// <param name="recordTo">The record to.</param>
    /// <param name="cancelToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>System.Threading.Tasks.Task.</returns>
    protected abstract Task ExecuteSingleRestQuery(
        ClientScriptIterationResults recordTo,
        CancellationToken cancelToken = default);

    /// <summary>
    /// Gets the HTTP client to use.
    /// </summary>
    /// <value>The HTTP client.</value>
    protected HttpClient HttpClient { get; }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!_isDisposed)
        {
            if (disposing)
            {
                this.HttpClient.Dispose();
            }

            _isDisposed = true;
        }
    }
}