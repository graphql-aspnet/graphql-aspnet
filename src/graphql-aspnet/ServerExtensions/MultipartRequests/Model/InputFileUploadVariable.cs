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
    using GraphQL.AspNet.Common;
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
        /// <param name="value">The file to attach to the variable, if known.</param>
        public InputFileUploadVariable(string name, FileUpload value)
            : base(name)
        {
            Validation.ThrowIfNull(value, nameof(value));
            this.MapKey = value.MapKey;
            this.Value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InputFileUploadVariable"/> class.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <param name="mapKey">The mapped key value (on a request) this variable represents.</param>
        public InputFileUploadVariable(string name, string mapKey)
            : base(name)
        {
            Validation.ThrowIfNullWhiteSpace(mapKey, nameof(mapKey));
            this.MapKey = mapKey;
        }

        /// <summary>
        /// Gets the map key assigned to this file upload on the request that created it.
        /// </summary>
        /// <value>The file's unique map key on its originating request.</value>
        public string MapKey { get; }

        /// <summary>
        /// Gets or sets a reference to the file parsed from a request.
        /// </summary>
        /// <value>The file reference.</value>
        public FileUpload Value { get; set; }

        /// <inheritdoc />
        public object ResolvedValue => this.Value;
    }
}