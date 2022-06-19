// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

#pragma warning disable SA1025 // Code should not contain multiple whitespace in a row
namespace GraphQL.AspNet.PlanGeneration.Document
{
    /// <summary>
    /// An enumeration depicting the various parts of a parsed query document.
    /// </summary>
    public enum DocumentPartType
    {

        Unknown                 = 0,

        OperationCollection     = 1 << 1,
        Operation               = 1 << 2,

        VariableCollection      = 1 << 3,
        Variable                = 1 << 4,

        FieldSelectionSet       = 1 << 5,
        FieldSelection          = 1 << 6,

        InputArgumentCollection = 1 << 7,
        InputArgument           = 1 << 8,
        SuppliedValue           = 1 << 9,

        Directive               = 1 << 10,

        FragmentCollection      = 1 << 11,
        Fragment                = 1 << 12,
    }
}