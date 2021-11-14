// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.PlanGeneration.Document.Parts.QueryInputValues
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.PlanGeneration.Resolvables;
    using GraphQL.AspNet.Parsing.SyntaxNodes;

    /// <summary>
    /// A base class providing common functionality for any "provided value" for an argument in a query document.
    /// </summary>
    [Serializable]
    public abstract class QueryInputValue : IDocumentPart, IResolvableItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryInputValue" /> class.
        /// </summary>
        /// <param name="node">The node that represents this input value in the user query document.</param>
        protected QueryInputValue(SyntaxNode node)
        {
            this.ValueNode = Validation.ThrowIfNullOrReturn(node, nameof(node));
        }

        /// <summary>
        /// Assigns the parent argument to this value.
        /// </summary>
        /// <param name="argument">The argument.</param>
        public void AssignParent(IInputValueDocumentPart argument)
        {
            this.OwnerArgument = Validation.ThrowIfNullOrReturn(argument, nameof(argument));
        }

        /// <summary>
        /// Adds the argument to the collection of arguments on this instance.
        /// </summary>
        /// <param name="child">The child.</param>
        public virtual void AddChild(IDocumentPart child)
        {
            throw new InvalidOperationException($"{this.GetType().FriendlyName()} cannot contain children of type '{child?.GetType()}'");
        }

        /// <summary>
        /// Gets the defined argument in the query document to which this value is associated.
        /// </summary>
        /// <value>The argument.</value>
        public IInputValueDocumentPart OwnerArgument { get; private set; }

        /// <summary>
        /// Gets the input value this instance is contained within. For example a list item or an argument
        /// of a complex object.
        /// </summary>
        /// <value>The container value.</value>
        public QueryInputValue OwnerValue { get; internal set; }

        /// <summary>
        /// Gets the value node from the query document that represents this input value.
        /// </summary>
        /// <value>The value node.</value>
        public SyntaxNode ValueNode { get; }

        /// <summary>
        /// Gets the child parts declared in this instance.
        /// </summary>
        /// <value>The children.</value>
        public virtual IEnumerable<IDocumentPart> Children => Enumerable.Empty<IDocumentPart>();
    }
}