// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ServerExtensions.MultipartRequests.Model
{
    using GraphQL.AspNet.Execution.Variables;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Resolvables;

    /// <summary>
    /// A user-supplied, graphql variable that represents an instance of a file uploaded on a query.
    /// </summary>
    public class InputFileUploadVariable : InputVariable, IResolvedScalarValue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InputFileUploadVariable"/> class.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <param name="value">The file to attach to the variable.</param>
        public InputFileUploadVariable(string name, FileUpload value)
            : base(name)
        {
            this.Value = value;
        }

        /// <summary>
        /// Gets a reference to the file parsed from a request.
        /// </summary>
        /// <value>The file reference.</value>
        public FileUpload Value { get; }

        /// <inheritdoc />
        public object ResolvedValue => this.Value;
    }
}