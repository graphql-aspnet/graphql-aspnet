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
        /// <summary>
        /// An unknown document type. This type indicates an error condition.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// A top level operation to execute (e.g. query, mutation etc.).
        /// </summary>
        Operation                       = 10,

        /// <summary>
        /// A variable declared on an operation.
        /// </summary>
        Variable                        = 20,

        /// <summary>
        /// The set of fields to select from the parent <see cref="Field"/>'s resolved value, if any.
        /// Fields that return leaf types will cannot contain a selection set.
        /// </summary>
        FieldSelectionSet               = 30,

        /// <summary>
        /// A single field of data to query from a given source object.
        /// </summary>
        Field                           = 40,

        /// <summary>
        /// An argument supplied to a <see cref="Field" /> or a <see cref="Directive"/>.
        /// </summary>
        Argument                        = 50,

        /// <summary>
        /// A supplied value to an <see cref="Argument"/>.
        /// </summary>
        SuppliedValue                   = 60,

        /// <summary>
        /// A directive attached to its parent document part, to be executed by the runtime prior
        /// to executing the document.
        /// </summary>
        Directive                       = 70,

        /// <summary>
        /// A fragment to be spread, declared inline in the parent <see cref="FieldSelectionSet"/>.
        /// </summary>
        InlineFragment                  = 80,

        /// <summary>
        /// A top level, formally declared fragment that can be spread into any allowed
        /// <see cref="FieldSelectionSet"/> within the document.
        /// </summary>
        NamedFragment                   = 90,

        /// <summary>
        /// A spread operation to inject a <see cref="NamedFragment"/> into this items parent
        /// parent <see cref="FieldSelectionSet"/>.
        /// </summary>
        FragmentSpread                  = 100,

        /// <summary>
        /// The document itself. Represents the top most part from which all other
        /// parts children or grandchildren etc.
        /// </summary>
        Document = 110,
    }
}