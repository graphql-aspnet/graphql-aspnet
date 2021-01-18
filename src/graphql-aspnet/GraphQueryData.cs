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
    using GraphQL.AspNet.Variables;

    /// <summary>
    /// An binding model representing an incoming, structured GraphQL query on a HTTP request.
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
        /// <value>The variables.</value>
        public InputVariableCollection Variables { get; set; }
    }
}