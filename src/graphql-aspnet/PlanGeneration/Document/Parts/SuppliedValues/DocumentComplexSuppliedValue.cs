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
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.PlanGeneration.Resolvables;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs.Values;

    /// <summary>
    /// An input value representing a complex input object read from a user's query document.
    /// </summary>
    [DebuggerDisplay("ComplexInputValue (Children = {Children.Count})")]
    internal class DocumentComplexSuppliedValue : DocumentSuppliedValue, IComplexSuppliedValueDocumentPart
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentComplexSuppliedValue" /> class.
        /// </summary>
        /// <param name="parentPart">The parent document part, if any, that owns this instance.</param>
        /// <param name="node">The node that represents this input value in the user query document.</param>
        /// <param name="key">An optional key indicating the name of this supplied value, if one was given.</param>
        public DocumentComplexSuppliedValue(IDocumentPart parentPart, ComplexValueNode node, string key = null)
            : base(parentPart, node, key)
        {
        }

        public bool TryGetField(string fieldName, out IResolvableValueItem foundField)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public IEnumerable<IResolvableValueItem> Fields => this
                .Children[DocumentPartType.SuppliedValue]
                .OfType<ISuppliedValueDocumentPart>();
    }
}