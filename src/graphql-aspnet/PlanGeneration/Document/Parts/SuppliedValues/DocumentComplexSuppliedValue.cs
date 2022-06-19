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
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.PlanGeneration.Resolvables;
    using GraphQL.AspNet.Parsing.SyntaxNodes;

    /// <summary>
    /// An input value representing a complex input object read from a user's query document.
    /// </summary>
    [DebuggerDisplay("ComplexInputValue (Arguments = {Arguments.Count})")]
    internal class DocumentComplexSuppliedValue : DocumentSuppliedValue, IComplexSuppliedValueDocumentPart
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentComplexSuppliedValue" /> class.
        /// </summary>
        /// <param name="node">The node that represents this input value in the user query document.</param>
        public DocumentComplexSuppliedValue(SyntaxNode node)
            : base(node)
        {
            this.Arguments = new DocumentInputArgumentCollection();
        }

        /// <inheritdoc />
        public override void AddChild(IDocumentPart child)
        {
            if (child is IInputArgumentDocumentPart qa)
            {
                this.AddArgument(qa);
            }
            else
            {
                base.AddChild(child);
            }
        }

        /// <inheritdoc />
        public void AddArgument(IInputArgumentDocumentPart argument)
        {
            this.Arguments.AddArgument(argument);
        }

        /// <inheritdoc />
        public bool TryGetField(string fieldName, out IResolvableItem field)
        {
            field = null;
            var found = this.Arguments.TryGetValue(fieldName, out var item);
            if (found)
                field = item.Value;

            return found;
        }

        /// <inheritdoc />
        public IEnumerable<KeyValuePair<string, IResolvableItem>> Fields
        {
            get
            {
                foreach (var argument in this.Arguments.Values)
                {
                    yield return new KeyValuePair<string, IResolvableItem>(
                        argument.Name,
                        argument.Value);
                }
            }
        }

        /// <inheritdoc />
        public IInputArgumentCollectionDocumentPart Arguments { get; }

        /// <inheritdoc />
        public override IEnumerable<IDocumentPart> Children
        {
            get
            {
                return this.Arguments.Values;
            }
        }
    }
}