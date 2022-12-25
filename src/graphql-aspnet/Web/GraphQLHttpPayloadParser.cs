// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Web
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution.Variables;
    using GraphQL.AspNet.Web.Exceptions;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// An implementation of the business rules that extract the data received on an
    /// <see cref="Microsoft.AspNetCore.Http.HttpContext"/> to create a <see cref="GraphQueryData" />
    /// object used by the graphql runtime.
    /// </summary>
    public class GraphQLHttpPayloadParser
    {
        /// <summary>
        /// An error message constant, in english, providing the text  to return to the caller when they use any HTTP action verb
        /// other than post.
        /// </summary>
        protected const string ERROR_USE_POST = "GraphQL queries should be executed as a POST request";

        /// <summary>
        /// An error message format constant, in english, providing the text to return to teh caller
        /// when an attempt to deserailize a json payload on a POST request fails.
        /// </summary>
        protected const string ERROR_POST_SERIALIZATION_ISSUE_FORMAT = "Unable to deserialize the POST body as a JSON object: {0}";

        /// <summary>
        /// An error message format constant, in english, providing the text to return to the caller
        /// when an attempt to deserialize the json payload that represents the variables collection, on the
        /// query string, fails.
        /// </summary>
        protected static readonly string ERROR_VARIABLE_PARAMETER_SERIALIZATION_ISSUE_FORMAT = $"Unable to deserialize the '{Constants.Web.QUERYSTRING_VARIABLES_KEY}' query string parameter: {{0}}";

        private static readonly JsonSerializerOptions _options;

        /// <summary>
        /// Initializes static members of the <see cref="GraphQLHttpPayloadParser"/> class.
        /// </summary>
        static GraphQLHttpPayloadParser()
        {
            // attempt to decode the body as a json object
            _options = new JsonSerializerOptions();
            _options.PropertyNameCaseInsensitive = true;
            _options.AllowTrailingCommas = true;
            _options.ReadCommentHandling = JsonCommentHandling.Skip;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphQLHttpPayloadParser"/> class.
        /// </summary>
        /// <param name="context">The context to generate from.</param>
        public GraphQLHttpPayloadParser(HttpContext context)
        {
            this.HttpContext = Validation.ThrowIfNullOrReturn(context, nameof(context));
        }

        /// <summary>
        /// Generates a query data object usable by the graphql runtime.
        /// </summary>
        /// <returns>GraphQueryData.</returns>
        public virtual async Task<GraphQueryData> ParseAsync()
        {
            GraphQueryData queryData = new GraphQueryData();

            // ------------------------------
            // Step 0: GET or POST only
            // ---------------------------
            if (!this.IsPostRequest && !this.IsGetRequest)
                throw new HttpContextParsingException(errorMessage: ERROR_USE_POST);

            // ------------------------------
            // Step 1: POST Body processing
            // ------------------------------
            // First attempt to decode the post body when applicable
            if (this.IsPostRequest)
            {
                try
                {
                    queryData = await this.DecodePostBodyAsync();
                }
                catch (JsonException ex)
                {
                    var message = string.Format(ERROR_POST_SERIALIZATION_ISSUE_FORMAT, ex.Message);
                    throw new HttpContextParsingException(errorMessage: message);
                }
            }

            // ------------------------------
            // Step 2:  Query String Updates
            // ------------------------------
            // Apply overrides with the query string parameters if they exist
            this.ApplyQueryStringOverrides(queryData);

            return queryData;
        }

        /// <summary>
        /// Attempts to extract the necessary keys from the query string of the http context
        /// and update the supplied query data object with the values found. Query string values not found
        /// are not applied to the supplied object.
        /// </summary>
        /// <param name="queryData">The query data object to update.</param>
        protected virtual void ApplyQueryStringOverrides(GraphQueryData queryData)
        {
            var httpQueryString = this.HttpContext.Request.Query;
            if (httpQueryString.ContainsKey(Constants.Web.QUERYSTRING_QUERY_KEY))
            {
                var query = httpQueryString[Constants.Web.QUERYSTRING_QUERY_KEY];
                if (!string.IsNullOrWhiteSpace(query))
                    queryData.Query = query;
            }

            if (httpQueryString.ContainsKey(Constants.Web.QUERYSTRING_OPERATIONNAME_KEY))
            {
                var operationName = httpQueryString[Constants.Web.QUERYSTRING_OPERATIONNAME_KEY];
                if (!string.IsNullOrWhiteSpace(operationName))
                    queryData.OperationName = operationName;
            }

            if (httpQueryString.ContainsKey(Constants.Web.QUERYSTRING_VARIABLES_KEY))
            {
                var variables = httpQueryString[Constants.Web.QUERYSTRING_VARIABLES_KEY];
                if (!string.IsNullOrWhiteSpace(variables))
                {
                    try
                    {
                        queryData.Variables = InputVariableCollection.FromJsonDocument(variables);
                    }
                    catch (JsonException ex)
                    {
                        var message = string.Format(ERROR_VARIABLE_PARAMETER_SERIALIZATION_ISSUE_FORMAT, ex.Message);
                        throw new HttpContextParsingException(errorMessage: message);
                    }
                }
            }
        }

        /// <summary>
        /// Attempts to deserialize the POST body of the httpcontext
        /// into a <see cref="GraphQueryData" /> object that can be processed by the GraphQL runtime.
        /// </summary>
        /// <returns>GraphQueryData.</returns>
        protected virtual async Task<GraphQueryData> DecodePostBodyAsync()
        {
            // if the content-type is set to graphql, treat
            // the whole body as the query string
            // -----
            // See: https://graphql.org/learn/serving-over-http/#http-methods-headers-and-body
            // -----
            if (this.IsGraphQLBody)
            {
                using var reader = new StreamReader(this.HttpContext.Request.Body, Encoding.UTF8, true, 1024, true);
                var query = await reader.ReadToEndAsync();
                return new GraphQueryData()
                {
                    Query = query,
                };
            }

            return await JsonSerializer.DeserializeAsync<GraphQueryData>(
                this.HttpContext.Request.Body,
                _options,
                this.HttpContext.RequestAborted)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the context being parsed by this instance.
        /// </summary>
        /// <value>The context.</value>
        protected HttpContext HttpContext { get; }

        /// <summary>
        /// Gets a value indicating whether the watched http context is a GET request.
        /// </summary>
        /// <value>A value indicating if the context is a GET request.</value>
        public virtual bool IsGetRequest => string.Equals(this.HttpContext.Request.Method, nameof(HttpMethod.Get), StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Gets a value indicating whether the watched http context is a POST request.
        /// </summary>
        /// <value>A value indicating if the context is a post request.</value>
        public virtual bool IsPostRequest => string.Equals(this.HttpContext.Request.Method, nameof(HttpMethod.Post), StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Gets a value indicating whether the content-type of the http request is set to <c>application/graphql</c>.
        /// </summary>
        /// <value>A value indicating if the http post body content type represents a graphql query.</value>
        public virtual bool IsGraphQLBody => string.Equals(this.HttpContext.Request.ContentType, Constants.Web.GRAPHQL_CONTENT_TYPE_HEADER_VALUE, StringComparison.OrdinalIgnoreCase);
    }
}