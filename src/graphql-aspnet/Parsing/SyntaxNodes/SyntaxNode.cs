// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Parsing.SyntaxNodes
{
    using System;
    using System.Collections.Generic;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Internal;
    using GraphQL.AspNet.Parsing.Lexing.Tokens;
    using GraphQL.AspNet.RulesEngine.RuleSets.DocumentConstruction.Steps;

    /// <summary>
    /// The <see cref="SyntaxNode"/> is a single element in a syntax tree generated from the
    /// lexed source document. It is a represention of a "thing" in a graphql source document.
    /// Groups of <see cref="LexicalToken"/> together when parsed become syntax nodes. Syntax nodes
    /// create a foundational framework from which an execution plan can be generated and ran against a schema.
    /// </summary>
    [Serializable]
    public abstract class SyntaxNode
    {
        private bool _childrenSet;
        private ISyntaxNodeList _nodeList;
        private int _childStartIndex;
        private int _childLength;

        /// <summary>
        /// Initializes a new instance of the <see cref="SyntaxNode" /> class.
        /// </summary>
        /// <param name="startLocation">The start location.</param>
        protected SyntaxNode(SourceLocation startLocation)
        {
            this.Location = Validation.ThrowIfNullOrReturn(startLocation, nameof(startLocation));
        }

        /// <summary>
        /// Sets the single provided child node as the ONLY child of this syntax node.
        /// Once set, no additional children can be set.
        /// </summary>
        /// <param name="syntaxNode">The syntax node.</param>
        public void SetChild(ISyntaxNodeList nodeList, SyntaxNode syntaxNode)
        {
            if (_childrenSet)
                throw new InvalidOperationException("Child nodes already defined, they cannot be edited once set");

            _childrenSet = true;
            if (syntaxNode == null)
                return;

            Validation.ThrowIfNull(nodeList, nameof(nodeList));
            var index = nodeList.PreserveNode(syntaxNode);
            _nodeList = nodeList;
            _childLength = 1;
            _childStartIndex = index;
        }

        /// <summary>
        /// Using the provided collection id, on the <see cref="MasterNodeList"/>,
        /// sets the contents of that collection as the children of this node.
        /// </summary>
        /// <remarks>
        /// Use -1 to indicate no children should be associated with this node.
        /// </remarks>
        /// <param name="collectionId">The collection identifier.</param>
        public void SetChildren(ISyntaxNodeList nodeList, int collectionId)
        {
            if (_childrenSet)
                throw new InvalidOperationException("Child nodes already defined, they cannot be edited once set");

            Validation.ThrowIfNull(nodeList, nameof(nodeList));
            _childrenSet = true;

            var result = nodeList.PreserveCollection(collectionId);
            _nodeList = nodeList;
            _childStartIndex = result.StartIndex;
            _childLength = result.Length;
        }

        /// <summary>
        /// Gets the friendly name of the node.
        /// </summary>
        /// <value>The name of the node.</value>
        protected virtual string NodeName => this.GetType().FriendlyName();

        /// <summary>
        /// Gets the child nodes owned by this instance, if any.
        /// </summary>
        /// <value>The children.</value>
        public IReadOnlyList<SyntaxNode> Children
        {
            get
            {
                if (_nodeList == null || _childLength == 0 || _childStartIndex < 0)
                    return null;

                return _nodeList.CreateSegment(_childStartIndex, _childLength);
            }
        }

        /// <summary>
        /// Gets the location in the source text where this node originated.
        /// </summary>
        /// <value>The location.</value>
        public SourceLocation Location { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return "Unknown";
        }
    }
}