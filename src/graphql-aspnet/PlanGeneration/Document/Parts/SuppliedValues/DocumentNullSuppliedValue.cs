// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.PlanGeneration.Document.Parts.SuppliedValues
{
    using System;
    using System.Diagnostics;
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts.Common;

    /// <summary>
    /// An input value representing that nothing/null was used as a supplied parameter for an input argument.
    /// </summary>
    [DebuggerDisplay("{Description}")]
    internal class DocumentNullSuppliedValue : DocumentSuppliedValue, INullSuppliedValueDocumentPart
    {
        public DocumentNullSuppliedValue(IDocumentPart parentPart, SourceLocation location, string key = null)
            : base(parentPart, location, key)
        {
        }

        /// <inheritdoc />
        public override bool IsEqualTo(ISuppliedValueDocumentPart value)
        {
            return value != null && value is INullSuppliedValueDocumentPart;
        }

        /// <inheritdoc />
        public override string Description => "Null Value";
    }
}