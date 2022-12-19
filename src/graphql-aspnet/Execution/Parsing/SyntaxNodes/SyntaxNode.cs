// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.Parsing.SyntaxNodes
{
    using System;
    using System.Diagnostics;
    using GraphQL.AspNet.Common.Source;

    /// <summary>
    /// A representing of a syntax node within a larger AST
    /// representing the query document.
    /// </summary>
    [DebuggerDisplay("{NodeType} ({Coordinates})")]
    internal readonly struct SyntaxNode : IEquatable<SyntaxNode>
    {
        /// <summary>
        /// Gets a syntax node that represents nothing.
        /// </summary>
        /// <value>The none.</value>
        public static SyntaxNode None { get; } = new SyntaxNode(SyntaxNodeType.Empty, SourceLocation.None);

        /// <summary>
        /// Initializes a new instance of the <see cref="SyntaxNode"/> struct.
        /// </summary>
        /// <param name="nodeType">The type of node this instance represents.</param>
        /// <param name="location">The location in the source text where this node originated.</param>
        [DebuggerStepperBoundary]
        public SyntaxNode(
            SyntaxNodeType nodeType,
            SourceLocation location)
            : this(nodeType, location, SyntaxNodeValue.None, SyntaxNodeValue.None, SyntaxNodeCoordinates.None)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SyntaxNode"/> struct.
        /// </summary>
        /// <param name="nodeType">The type of node this instance represents.</param>
        /// <param name="location">The location in the source text where this node originated.</param>
        /// <param name="primaryValue">The primary text value of this node.</param>
        [DebuggerStepperBoundary]
        public SyntaxNode(
            SyntaxNodeType nodeType,
            SourceLocation location,
            SyntaxNodeValue primaryValue)
            : this(nodeType, location, primaryValue, SyntaxNodeValue.None, SyntaxNodeCoordinates.None)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SyntaxNode"/> struct.
        /// </summary>
        /// <param name="nodeType">The type of node this instance represents.</param>
        /// <param name="location">The location in the source text where this node originated.</param>
        /// <param name="primaryValue">The primary text value of this node.</param>
        /// <param name="coords">The coordinates of this node within the tree.</param>
        [DebuggerStepperBoundary]
        public SyntaxNode(
            SyntaxNodeType nodeType,
            SourceLocation location,
            SyntaxNodeValue primaryValue,
            SyntaxNodeCoordinates coords)
            : this(nodeType, location, primaryValue, SyntaxNodeValue.None, coords)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SyntaxNode" /> struct.
        /// </summary>
        /// <param name="nodeType">The type of node this instance represents.</param>
        /// <param name="location">The location in the source text where this node originated.</param>
        /// <param name="coords">The coordinates of this node within the tree.</param>
        [DebuggerStepperBoundary]
        public SyntaxNode(
            SyntaxNodeType nodeType,
            SourceLocation location,
            SyntaxNodeCoordinates coords)
            : this(nodeType, location, SyntaxNodeValue.None, SyntaxNodeValue.None, coords)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SyntaxNode" /> struct.
        /// </summary>
        /// <param name="nodeType">The type of node this instance represents.</param>
        /// <param name="primaryValue">The primary text value of this node.</param>
        [DebuggerStepperBoundary]
        public SyntaxNode(
            SyntaxNodeType nodeType,
            SyntaxNodeValue primaryValue)
            : this(nodeType, default, primaryValue, SyntaxNodeValue.None, SyntaxNodeCoordinates.None)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SyntaxNode" /> struct.
        /// </summary>
        /// <param name="nodeType">The type of node this instance represents.</param>
        /// <param name="primaryValue">The primary text value of this node.</param>
        /// <param name="secondaryValue">The secondary text value of this node.</param>
        public SyntaxNode(
            SyntaxNodeType nodeType,
            SyntaxNodeValue primaryValue,
            SyntaxNodeValue secondaryValue)
            : this(nodeType, default, primaryValue, secondaryValue, SyntaxNodeCoordinates.None)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SyntaxNode" /> struct.
        /// </summary>
        /// <param name="nodeType">The type of node this instance represents.</param>
        /// <param name="location">The location in the source text where this node originated.</param>
        /// <param name="primaryValue">The primary text value of this node.</param>
        /// <param name="secondaryValue">The secondary text value of this node.</param>
        [DebuggerStepperBoundary]
        public SyntaxNode(
            SyntaxNodeType nodeType,
            SourceLocation location,
            SyntaxNodeValue primaryValue,
            SyntaxNodeValue secondaryValue)
            : this(nodeType, location, primaryValue, secondaryValue, SyntaxNodeCoordinates.None)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SyntaxNode"/> struct.
        /// </summary>
        /// <param name="nodeType">The type of node this instance represents.</param>
        /// <param name="location">The location in the source text where this node originated.</param>
        /// <param name="primaryValue">The primary text value of this node.</param>
        /// <param name="secondaryValue">The secondary text value of this node.</param>
        /// <param name="coords">The coordinates of this node within the tree.</param>
        public SyntaxNode(
            SyntaxNodeType nodeType,
            SourceLocation location,
            SyntaxNodeValue primaryValue,
            SyntaxNodeValue secondaryValue,
            SyntaxNodeCoordinates coords)
        {
            this.NodeType = nodeType;
            this.Location = location;
            this.PrimaryValue = primaryValue;
            this.SecondaryValue = secondaryValue;
            this.Coordinates = coords;
        }

        /// <summary>
        /// Clones this node into a new copy. Any values supplied (i.e. not null)
        /// will be used instead of the values contained in this instance.
        /// </summary>
        /// <param name="location">The new location value to use.</param>
        /// <param name="primary">The new primary value to use.</param>
        /// <param name="secondary">The new secondary value to use.</param>
        /// <param name="coords">The new coordinates of the node in the master syntax tree.</param>
        /// <returns>SynNode.</returns>
        public SyntaxNode Clone(
            SourceLocation? location = null,
            SyntaxNodeValue? primary = null,
            SyntaxNodeValue? secondary = null,
            SyntaxNodeCoordinates? coords = null)
        {
            return new SyntaxNode(
                this.NodeType,
                location ?? this.Location,
                primary ?? this.PrimaryValue,
                secondary ?? this.SecondaryValue,
                coords ?? this.Coordinates);
        }

        /// <summary>
        /// Gets the location information of this node and its children in the
        /// master syntax tree.
        /// </summary>
        /// <value>The coordinates in the master tree.</value>
        public SyntaxNodeCoordinates Coordinates { get; }

        /// <summary>
        /// Gets the primary text block assigned to this node.
        /// </summary>
        /// <value>The primary value.</value>
        public SyntaxNodeValue PrimaryValue { get; }

        /// <summary>
        /// Gets the secondary text block assigned to this node.
        /// </summary>
        /// <value>The secondary value.</value>
        public SyntaxNodeValue SecondaryValue { get; }

        /// <summary>
        /// Gets the formal type assigned to this syntax node.
        /// </summary>
        /// <value>The type of the node.</value>
        public SyntaxNodeType NodeType { get; }

        /// <summary>
        /// Gets the location in the source text that generated this
        /// syntax node.
        /// </summary>
        /// <value>The location.</value>
        public SourceLocation Location { get; }

        /// <summary>
        /// Gets a value indicating whether this node is the root node in a given tree.
        /// </summary>
        /// <value><c>true</c> if this instance is a root node; otherwise, <c>false</c>.</value>
        public bool IsRootNode => this.Coordinates.BlockIndex == -1;

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return (this.PrimaryValue, this.SecondaryValue, this.NodeType).GetHashCode();
        }

        /// <inheritdoc />
        public bool Equals(SyntaxNode other)
        {
            return this.NodeType == other.NodeType
             && this.PrimaryValue.Equals(other.PrimaryValue)
             && this.SecondaryValue.Equals(other.SecondaryValue)
             && this.Coordinates.Equals(other.Coordinates);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return
                obj is SyntaxNode sn
                && this.Equals(sn);
        }

        /// <summary>
        /// Implements the == operator.
        /// </summary>
        /// <param name="lhs">The left hand side of the equality check.</param>
        /// <param name="rhs">The right hand side of the equality check.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(SyntaxNode lhs, SyntaxNode rhs) => lhs.Equals(rhs);

        /// <summary>
        /// Implements the != operator.
        /// </summary>
        /// <param name="lhs">The left hand side of the equality check.</param>
        /// <param name="rhs">The right hand side of the equality check.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(SyntaxNode lhs, SyntaxNode rhs) => !(lhs == rhs);
    }
}