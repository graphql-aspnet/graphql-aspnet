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
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GraphQL.AspNet.SubscriberLoadTest.Models.Models.ClientModels;
using GraphQL.AspNet.SubscriberLoadTest.TestConsole.Clients.Common;

/// <summary>
/// A request query script that performs a GET request to retrieve a donut.
/// </summary>
public class RetrieveDonutRestQuery : RestScriptBase
{
    private readonly string _urlBase;
    private bool _isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="RetrieveDonutRestQuery" /> class.
    /// </summary>
    /// <param name="urlBase">The url to send rest requests to.</param>
    /// <param name="scriptNumber">The individual script number of this instance.</param>
    /// <param name="categorySuffix">The execution category suffix
    /// to apply to this individual script instance.</param>
    public RetrieveDonutRestQuery(
        string urlBase,
        int scriptNumber,
        string categorySuffix = "")
        : base(scriptNumber, "RestDonut", categorySuffix)
    {
        _urlBase = urlBase;
        if (_urlBase.EndsWith("/"))
            _urlBase = _urlBase.Substring(0, _urlBase.Length - 1);
    }

    /// <inheritdoc />
    protected override async Task ExecuteSingleRestQuery(ClientScriptIterationResults recordTo, CancellationToken cancelToken = default)
    {
        if (_isDisposed)
            throw new ObjectDisposedException(nameof(RetrieveDonutRestQuery));

        var fullUrl = _urlBase + "/api/donuts/1";
        try
        {
            var result = await this.HttpClient.GetAsync(fullUrl, cancelToken);

            if (result.StatusCode != System.Net.HttpStatusCode.OK)
            {
                recordTo.AddResultsByStatusCode((int)result.StatusCode);
                return;
            }

            var serializedDonut = await result.Content.ReadAsStringAsync();

            Donut donut;
            donut = JsonSerializer.Deserialize<Donut>(serializedDonut, JSON_OPTIONS);

            if (!string.IsNullOrWhiteSpace(donut?.Id)
                && !string.IsNullOrWhiteSpace(donut?.Name)
                && !string.IsNullOrWhiteSpace(donut?.Flavor))
            {
                recordTo.AddResults("success");
            }
            else
            {
                recordTo.AddResults("failure");
            }
        }
        catch (Exception ex)
        {
            this.RecordException(recordTo, ex);
        }
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
            }

            _isDisposed = true;
        }
    }
}