// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet
{
    using System.Text.Json.Serialization;
    using GraphQL.AspNet.Common.Json;
    using GraphQL.AspNet.Execution.Variables;
    using GraphQL.AspNet.Interfaces.Execution.Variables;

    /// <summary>
    /// A raw data package representing the various inputs to the graphql runtime.
    /// </summary>
    public class GraphQueryData
    {
        /// <summary>
        /// Gets a singleton of an empty query data set.
        /// </summary>
        /// <value>The empty.</value>
        public static GraphQueryData Empty { get; }

        /// <summary>
        /// Initializes static members of the <see cref="GraphQueryData"/> class.
        /// </summary>
        static GraphQueryData()
        {
            GraphQueryData.Empty = new GraphQueryData()
            {
                OperationName = null,
                Query = null,
                Variables = new InputVariableCollection(),
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphQueryData"/> class.
        /// </summary>
        public GraphQueryData()
        {
        }

        /// <summary>
        /// Gets or sets the name of the operation in the query document to execute. Must be included
        /// when the document defines more than one operation.
        /// </summary>
        /// <value>The name of the operation.</value>
        public string OperationName { get; set; }

        /// <summary>
        /// Gets or sets the query document text to execute, as supplied by the request.
        /// </summary>
        /// <value>The query.</value>
        public string Query { get; set; }

        /// <summary>
        /// Gets or sets the variables being supplied on the request, if any.
        /// </summary>
        /// <value>The variables provided on the request.</value>
        [JsonConverter(typeof(IInputVariableCollectionConverter))]
        public IInputVariableCollection Variables { get; set; }
    }
}