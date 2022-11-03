// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Parsing2
{
    using System;
    using System.Diagnostics;
    using GraphQL.AspNet.Common.Source;

    /// <summary>
    /// A representing of a syntax node within a larger AST
    /// representing the query document.
    /// </summary>
    [DebuggerDisplay("{PrimaryValue} ({Coordinates})")]
    public readonly struct SynNode : IEquatable<SynNode>
    {

        public static SynNode None { get; } = new SynNode(SynNodeType.Empty, SourceLocation.None);

        [DebuggerStepperBoundary]
        public SynNode(
            SynNodeType nodeType,
            SourceLocation location)
            : this(nodeType, location, SynNodeValue.None, SynNodeValue.None, SynNodeCoordinates.None)
        {
        }


        [DebuggerStepperBoundary]
        public SynNode(
            SynNodeType nodeType,
            SourceLocation location,
            SynNodeValue primaryValue)
            : this(nodeType, location, primaryValue, SynNodeValue.None, SynNodeCoordinates.None)
        {
        }


        [DebuggerStepperBoundary]
        public SynNode(
            SynNodeType nodeType,
            SourceLocation location,
            SynNodeValue primaryValue,
            SynNodeCoordinates coords)
            : this(nodeType, location, primaryValue, SynNodeValue.None, coords)
        {
        }


        [DebuggerStepperBoundary]
        public SynNode(
            SynNodeType nodeType,
            SourceLocation location,
            SynNodeCoordinates coords)
            : this(nodeType, location, SynNodeValue.None, SynNodeValue.None, coords)
        {
        }


        [DebuggerStepperBoundary]
        public SynNode(
            SynNodeType nodeType,
            SynNodeValue primaryValue)
            : this(nodeType, default, primaryValue, SynNodeValue.None, SynNodeCoordinates.None)
        {
        }

        public SynNode(
            SynNodeType nodeType,
            SynNodeValue primaryValue,
            SynNodeValue secondaryValue)
            : this(nodeType, default, primaryValue, secondaryValue, SynNodeCoordinates.None)
        {
        }

        [DebuggerStepperBoundary]
        public SynNode(
            SynNodeType nodeType,
            SourceLocation location,
            SynNodeValue primaryValue,
            SynNodeValue secondaryValue)
            : this(nodeType, location, primaryValue, secondaryValue, SynNodeCoordinates.None)
        {
        }

        public SynNode(
            SynNodeType nodeType,
            SourceLocation location,
            SynNodeValue primaryValue,
            SynNodeValue secondaryValue,
            SynNodeCoordinates coords)
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
        public SynNode Clone(
            SourceLocation? location = null,
            SynNodeValue? primary = null,
            SynNodeValue? secondary = null,
            SynNodeCoordinates? coords = null)
        {
            return new SynNode(
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
        public SynNodeCoordinates Coordinates { get; }

        /// <summary>
        /// Gets the primary value block assigned to this node.
        /// </summary>
        /// <value>The primary value.</value>
        public SynNodeValue PrimaryValue { get; }

        /// <summary>
        /// Gets the secondary value block assigned to this node.
        /// </summary>
        /// <value>The secondary value.</value>
        public SynNodeValue SecondaryValue { get; }

        /// <summary>
        /// Gets the formal type assigned to this syntax node.
        /// </summary>
        /// <value>The type of the node.</value>
        public SynNodeType NodeType { get; }

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
        public bool Equals(SynNode other)
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
                obj is SynNode sn
                && this.Equals(sn);
        }

        /// <summary>
        /// Implements the == operator.
        /// </summary>
        /// <param name="lhs">The left hand side of the equality check.</param>
        /// <param name="rhs">The right hand side of the equality check.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(SynNode lhs, SynNode rhs) => lhs.Equals(rhs);

        /// <summary>
        /// Implements the != operator.
        /// </summary>
        /// <param name="lhs">The left hand side of the equality check.</param>
        /// <param name="rhs">The right hand side of the equality check.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(SynNode lhs, SynNode rhs) => !(lhs == rhs);
    }
}