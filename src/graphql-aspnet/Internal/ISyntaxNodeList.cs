// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Internal
{
    using System;
    using GraphQL.AspNet.Parsing.SyntaxNodes;

    /// <summary>
    /// A segmented list of syntax nodes, centrally managed.
    /// </summary>
    public interface ISyntaxNodeList : IDisposable
    {
        /*
         * This specially managed list tracks ALL allocated syntax nodes for a given syntax tree
         * regardless of where they exist in said tree. This is to
         * reduce the heap allocations and prevent unnecessary GC pressure
         * under high loads.
         */

        /// <summary>
        /// Begins a new collection of nodes, returning the numeric id identifying
        /// the collection.
        /// </summary>
        /// <returns>System.Int32.</returns>
        int BeginTempCollection();

        /// <summary>
        /// Adds the node to the temporary collection. A collection must have
        /// already been started with <see cref="BeginTempCollection"/>.
        /// </summary>
        /// <param name="collectionId">The collection id.</param>
        /// <param name="node">The node.</param>
        void AddNodeToTempCollection(int collectionId, SyntaxNode node);

        /// <summary>
        /// For any temporary allocated collection, of a given id, stores that collection
        /// permanantly and frees up the temporary entity.
        /// </summary>
        /// <param name="collectionId">The collection identifier.</param>
        /// <returns>An index in <see cref="AllNodes" /> as well as a length of contiguous nodes where the nodes
        /// where stored.</returns>
        (int StartIndex, int Length) PreserveCollection(int collectionId);

        /// <summary>
        /// Creates a subsegment of nodes within the master collection.
        /// </summary>
        /// <param name="startIndex">The start index of the segment.</param>
        /// <param name="length">The length of the segment.</param>
        /// <returns>ArraySegment&lt;SyntaxNode&gt;.</returns>
        ArraySegment<SyntaxNode> CreateSegment(int startIndex, int length);

        /// <summary>
        /// Preserves the single node into the master collection.
        /// </summary>
        /// <param name="syntaxNode">The syntax node.</param>
        /// <returns>The index of the node in the master collection.</returns>
        int PreserveNode(SyntaxNode syntaxNode);

        /// <summary>
        /// Gets all nodes currently stored in this list.
        /// </summary>
        /// <value>All nodes.</value>
        SyntaxNode[] AllNodes { get; }
    }
}