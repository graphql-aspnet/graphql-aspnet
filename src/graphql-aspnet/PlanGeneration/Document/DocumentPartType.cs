// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.PlanGeneration.Document
{
    /// <summary>
    /// An enumeration depicting the various parts of a parsed query document.
    /// </summary>
    public enum DocumentPartType
    {
        Unknown = 0,
        Operation,
        Variable,
        VariableCollection,
        FieldSelection,
        FieldSelectionSet,
        SuppliedValue,
        InputArgument,
        Directive,
        Fragment,
        FragmentCollection,
        OperationCollection,
        InputArgumentCollection,
    }
}