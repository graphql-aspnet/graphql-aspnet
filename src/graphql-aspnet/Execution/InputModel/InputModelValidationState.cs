// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.InputModel
{
    /// <summary>
    /// An enumeraton indicating the validation state of a single model item.
    /// </summary>
    public enum InputModelValidationState
    {
        Unvalidated,
        Skipped,
        Invalid,
        Valid,
    }
}