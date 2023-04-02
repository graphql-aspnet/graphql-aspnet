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
    using System.Collections.Generic;
    using GraphQL.AspNet.Execution.Variables;

    /// <summary>
    /// An extended <see cref="InputVariableCollection"/> with additional details
    /// for finding and mapping uploaded files into the variable collection.
    /// </summary>
    internal class FileMappedInputVariableCollection : InputVariableCollection
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileMappedInputVariableCollection"/> class.
        /// </summary>
        public FileMappedInputVariableCollection()
        {
            this.FileUploadVariables = new List<InputFileUploadVariable>();
        }

        /// <summary>
        /// Gets a collection of file upload variables created and tracked as part of this variable colleciton.
        /// </summary>
        /// <value>The file upload variables contained in this collection.</value>
        public List<InputFileUploadVariable> FileUploadVariables { get; }
    }
}