// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ServerExtensions.MultipartRequests.Model.Json
{
    using GraphQL.AspNet.Execution.Variables;
    using GraphQL.AspNet.Execution.Variables.Json;
    using GraphQL.AspNet.Interfaces.Execution.Variables;

    /// <summary>
    /// A special instance of the <see cref="InputVariableCollectionConverter"/> that can inject
    /// file upload variables into the created collection.
    /// </summary>
    public class FileMappedInputVariableCollectionConverter : InputVariableCollectionConverter
    {
        /// <inheritdoc />
        protected override InputVariableCollection CreateCollection()
        {
            return new FileMappedInputVariableCollection();
        }

        /// <inheritdoc />
        protected override void OnVariableCreated(IInputVariableCollection variableCollection, IInputVariable variable)
        {
            if (variable is InputFileUploadVariable ifuv)
            {
                if (variableCollection is FileMappedInputVariableCollection fmi)
                    fmi.FileUploadVariables.Add(ifuv);
            }
        }

        /// <inheritdoc />
        protected override IInputVariable CreateStringVariable(string name, string value)
        {
            if (value.StartsWith(MultipartRequestConstants.Protected.FILE_MARKER_PREFIX))
            {
                var mappedFileName = value.Substring(MultipartRequestConstants.Protected.FILE_MARKER_PREFIX.Length);
                return new InputFileUploadVariable(name, mappedFileName);
            }

            return base.CreateStringVariable(name, value);
        }
    }
}