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
    /// A representation of a list of other input values for a single argument.
    /// </summary>
    [DebuggerDisplay("ListValue (Count = {ListItems.Count})")]
    public class DocumentListSuppliedValue : DocumentSuppliedValue, IListSuppliedValueDocumentPart
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentListSuppliedValue" /> class.
        /// </summary>
        /// <param name="node">The node that represents this input value in the user query document.</param>
        public DocumentListSuppliedValue(SyntaxNode node)
            : base(node)
        {
            this.ListItems = new List<ISuppliedValueDocumentPart>();
        }

        /// <inheritdoc />
        public override void AddChild(IDocumentPart child)
        {
            if (child is ISuppliedValueDocumentPart suppliedValue)
            {
                this.ListItems.Add(suppliedValue);
                suppliedValue.ParentValue =this;
                suppliedValue.Owner = this.Owner;
            }
            else
            {
                base.AddChild(child);
            }
        }

        /// <inheritdoc />
        IEnumerable<IResolvableItem> IResolvableList.ResolvableListItems => this.ListItems;

        /// <inheritdoc />
        public IList<ISuppliedValueDocumentPart> ListItems { get; }

        /// <inheritdoc />
        public override IEnumerable<IDocumentPart> Children
        {
            get
            {
                return this.ListItems;
            }
        }
    }
}