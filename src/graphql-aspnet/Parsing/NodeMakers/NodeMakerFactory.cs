// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Parsing.NodeMakers
{
    using System;
    using System.Collections.Generic;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Parsing.Lexing.Tokens;
    using GraphQL.AspNet.Parsing.NodeMakers.FieldMakers;
    using GraphQL.AspNet.Parsing.NodeMakers.ValueMakers;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Fragments;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs.Values;

    /// <summary>
    /// A factory that serves up "makers" that convert a series of <see cref="LexicalToken"/> into
    /// functional <see cref="SyntaxNode"/>.
    /// </summary>
    public static class NodeMakerFactory
    {
        private static readonly Dictionary<Type, ISyntaxNodeMaker> MAKERS = new Dictionary<Type, ISyntaxNodeMaker>();

        /// <summary>
        /// Initializes static members of the <see cref="NodeMakerFactory"/> class.
        /// </summary>
        static NodeMakerFactory()
        {
            MAKERS.Add(typeof(OperationTypeNode), OperationTypeNodeMaker.Instance);
            MAKERS.Add(typeof(NamedFragmentNode), NamedFragmentNodeMaker.Instance);
            MAKERS.Add(typeof(VariableNode), VariableNodeMaker.Instance);
            MAKERS.Add(typeof(VariableCollectionNode), VariableCollectionNodeMaker.Instance);
            MAKERS.Add(typeof(FieldCollectionNode), FieldCollectionNodeMaker.Instance);
            MAKERS.Add(typeof(InputValueNode), InputValueNodeMaker.Instance);
            MAKERS.Add(typeof(InputItemNode), InputItemNodeMaker.Instance);
            MAKERS.Add(typeof(InputItemCollectionNode), InputItemCollectionNodeMaker.Instance);
            MAKERS.Add(typeof(DirectiveNode), DirectiveNodeMaker.Instance);
        }

        /// <summary>
        /// Creates the maker.
        /// </summary>
        /// <typeparam name="TSyntaxNodeType">The type of the t syntax node type.</typeparam>
        /// <returns>ISyntaxNodeMaker.</returns>
        public static ISyntaxNodeMaker CreateMaker<TSyntaxNodeType>()
        {
            return NodeMakerFactory.CreateMaker(typeof(TSyntaxNodeType));
        }

        /// <summary>
        /// Creates the maker.
        /// </summary>
        /// <param name="nodeType">Type of the node.</param>
        /// <returns>ISyntaxNodeMaker.</returns>
        private static ISyntaxNodeMaker CreateMaker(Type nodeType)
        {
            Validation.ThrowIfNull(nodeType, nameof(nodeType));
            if (MAKERS.ContainsKey(nodeType))
                return MAKERS[nodeType];

            throw new ArgumentOutOfRangeException($"No {typeof(ISyntaxNodeMaker).FriendlyName()} exists for the type {nodeType.FriendlyName()}");
        }
    }
}