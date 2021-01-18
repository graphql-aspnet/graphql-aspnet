// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Benchmarks.Benchmarks
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using BenchmarkDotNet.Attributes;
    using GraphQL.AspNet.Benchmarks.Model;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Configuration.Mvc;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Middleware.QueryExecution;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Variables;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// A collection of query execution tests to run to profile the speed of
    /// graphql-aspnet.
    /// </summary>
    [Config(typeof(BenchmarkConfiguration))]
    public class ExecuteQueries
    {
        private IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes the graphql environment/schema to be benchmarked.
        /// </summary>
        [GlobalSetup]
        public void InitializeEnvironment()
        {
            var serviceCollection = new ServiceCollection() as IServiceCollection;

            // misc services for resolving data
            serviceCollection.AddScoped<IMusicService, MusicService>();
            serviceCollection.AddSingleton<IMusicRepository, MusicRepository>();

            // schema setup
            Action<SchemaOptions> configureOptions = (SchemaOptions options) =>
            {
                options.AddGraphType<Artist>();
                options.AddGraphType<MusicGenre>();
                options.AddGraphType<Record>();
                options.AddGraphType<RecordCompany>();
                options.AddGraphType<MusicController>();
            };

            var injector = new GraphQLSchemaInjector<GraphSchema>(serviceCollection, configureOptions);
            injector.ConfigureServices();

            _serviceProvider = serviceCollection.BuildServiceProvider();
            injector.UseSchema(_serviceProvider);
        }

        /// <summary>
        /// Executes the query.
        /// </summary>
        /// <param name="queryText">The query text.</param>
        /// <param name="jsonText">The json text.</param>
        /// <returns>Task.</returns>
        private async Task ExecuteQuery(string queryText, string jsonText = "{}")
        {
            // top level services to execute the query
            var queryPipeline = _serviceProvider.GetService<ISchemaPipeline<GraphSchema, GraphQueryExecutionContext>>();
            var writer = _serviceProvider.GetService<IGraphResponseWriter<GraphSchema>>();

            // parse the json doc, simulating a request recieved to the QueryController
            var inputVars = InputVariableCollection.FromJsonDocument(jsonText);

            var query = new GraphQueryData()
            {
                Query = queryText,
                Variables = inputVars ?? InputVariableCollection.Empty,
            };

            var request = new GraphOperationRequest(query);
            var context = new GraphQueryExecutionContext(request, _serviceProvider, null);

            // execute
            await queryPipeline.InvokeAsync(context, default);

            var response = context.Result;
            if (response.Messages.Count > 0)
                throw new InvalidOperationException("Query failed: " + response.Messages[0].Message);

            // simulate writing hte result to an output stream
            string result = null;
            using (var memStream = new MemoryStream())
            {
                await writer.WriteAsync(memStream, response);
                memStream.Seek(0, SeekOrigin.Begin);
                using (var streamReader = new StreamReader(memStream))
                    result = streamReader.ReadToEnd();
            }

            if (result == null)
            {
                throw new InvalidOperationException("Query failed: No Response was serialized");
            }
        }

        /// <summary>
        /// Simulates a simple object look up returning a single object given a set of inputs.
        /// </summary>
        /// <returns>Task.</returns>
        [Benchmark]
        public Task SingleObjectQuery()
        {
            return ExecuteQuery(
                @"query {
                    artist(id: 2){
                        id
                        recordCompanyId
                    name
                    }
                }");
        }

        /// <summary>
        /// Represents multiple controller action invocations in single query (3 instances: artist 2, artist 1, artist search) along
        /// with the processing of a variable (creation, validation, resolving etc.)
        /// </summary>
        /// <returns>Task.</returns>
        [Benchmark]
        public Task MultiActionMethodQuery()
        {
            return ExecuteQuery(
                @"query MultiArtistQuery($var1: String){
                    artist1: artist(id: 2){
                        id
                        recordCompanyId
                        name
                    }
                    artist2: artist(id: 1){
                        id
                        recordCompanyId
                        name
                    }
                    allArtists: artists(searchText: $var1){
                        id
                        name
                    }
                }",
                "{\"var1\": \"que\" }");
        }

        /// <summary>
        /// Represents field requests through type extensions. i.e. A secondary controller method invocation.
        /// </summary>
        /// <returns>Task.</returns>
        [Benchmark]
        public Task TypeExtensionQuery()
        {
            // "records" is a a batch extension on the 'Artist' graph type
            return ExecuteQuery(@"
                    query {
                    artists(searchText: ""queen"") {
                        id
                        name
                        records {
                            id
                            name
                            genre {
                                id
                                name
                            }
                        }
                    }
                }");
        }
    }
}