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
    public class DocumentComplexSuppliedValue : DocumentSuppliedValue, IQueryArgumentContainerDocumentPart, IResolvableFieldSet
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
            if (child is IQueryArgumentDocumentPart qa)
            {
                this.AddArgument(qa);
            }
            else
            {
                base.AddChild(child);
            }
        }

        /// <summary>
        /// Adds the argument to the collection of arguments on this instance.
        /// </summary>
        /// <param name="argument">The argument.</param>
        public void AddArgument(IQueryArgumentDocumentPart argument)
        {
            this.Arguments.AddArgument(argument);
        }

        /// <summary>
        /// Attempts to retrieve a field by its name.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="field">The field that was found, if any.</param>
        /// <returns><c>true</c> if the field was found and successfully returned, <c>false</c> otherwise.</returns>
        public bool TryGetField(string fieldName, out IResolvableItem field)
        {
            field = null;
            var found = this.Arguments.TryGetValue(fieldName, out var item);
            if (found)
                field = item.Value;

            return found;
        }

        /// <summary>
        /// Gets the collection of fields defined on this instance.
        /// </summary>
        /// <value>The fields.</value>
        public IEnumerable<KeyValuePair<string, IResolvableItem>> Fields
        {
            get
            {
                foreach (var argument in this.Arguments.Values)
                {
                    yield return new KeyValuePair<string, IResolvableItem>(argument.Name, argument.Value);
                }
            }
        }

        /// <summary>
        /// Gets a collection of input arguments arguments that have been declared in the query document that should be
        /// applied to this field.
        /// </summary>
        /// <value>The arguments.</value>
        public IQueryInputArgumentCollectionDocumentPart Arguments { get; }

        /// <summary>
        /// Gets the child parts declared in this instance.
        /// </summary>
        /// <value>The children.</value>
        public override IEnumerable<IDocumentPart> Children
        {
            get
            {
                return this.Arguments.Values;
            }
        }
    }
}