// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

#pragma warning disable SA1025 // Code should not contain multiple whitespace in a row
namespace GraphQL.AspNet.Execution.QueryPlans.Document
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
        /// A variable declared on an <see cref="Operation"/>.
        /// </summary>
        Variable                        = 20,

        /// <summary>
        /// The set of fields to select from the parent <see cref="Field"/>'s resolved value, if any.
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
        /// A field of data supplied to a complex INPUT_OBJECT assigned to an <see cref="Argument"/>.
        /// </summary>
        InputField                      = 60,

        /// <summary>
        /// A supplied value to an <see cref="Argument"/> or <see cref="InputField"/>, this may represent
        /// a scalar or complex value.
        /// </summary>
        SuppliedValue                   = 70,

        /// <summary>
        /// A directive attached to its parent document part, to be executed by the runtime prior
        /// to executing the document.
        /// </summary>
        Directive                       = 80,

        /// <summary>
        /// A fragment to be spread, declared inline within  <see cref="FieldSelectionSet"/>.
        /// </summary>
        InlineFragment                  = 90,

        /// <summary>
        /// A top level, formally declared, fragment that can be spread into any allowed
        /// <see cref="FieldSelectionSet"/> within the document.
        /// </summary>
        NamedFragment                   = 100,

        /// <summary>
        /// A spread operation to inject a <see cref="NamedFragment"/> into this items parent
        /// parent <see cref="FieldSelectionSet"/>.
        /// </summary>
        FragmentSpread                  = 110,

        /// <summary>
        /// The document itself. Represents the top most part from which all other
        /// parts are children or grandchildren etc.
        /// </summary>
        Document                        = 120,
    }
}