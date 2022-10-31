// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Parsing2.NodeBuilders
{
    using System;
    using System.Collections.Generic;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Parsing2.NodeBuilders.Inputs;

    public class NodeBuilderFactory
    {
        private static readonly Dictionary<SynNodeType, ISynNodeBuilder> BUILDERS;

        /// <summary>
        /// Initializes static members of the <see cref="NodeBuilderFactory"/> class.
        /// </summary>
        static NodeBuilderFactory()
        {
            BUILDERS = new Dictionary<SynNodeType, ISynNodeBuilder>();

            BUILDERS.Add(SynNodeType.Operation, OperationNodeBuilder.Instance);
            BUILDERS.Add(SynNodeType.NamedFragment, NamedFragmentNodeBuilder.Instance);
            BUILDERS.Add(SynNodeType.Variable, VariableNodeBuilder.Instance);
            BUILDERS.Add(SynNodeType.VariableCollection, VariableCollectionNodeBuilder.Instance);
            BUILDERS.Add(SynNodeType.FieldCollection, FieldCollectionNodeBuilder.Instance);
            BUILDERS.Add(SynNodeType.InputValue, InputValueNodeBuilder.Instance);
            BUILDERS.Add(SynNodeType.InputItem, InputItemNodeBuilder.Instance);
            BUILDERS.Add(SynNodeType.InputItemCollection, InputItemCollectionNodeBuilder.Instance);
            BUILDERS.Add(SynNodeType.Directive, DirectiveNodeBuilder.Instance);
        }

        /// <summary>
        /// Creates the maker.
        /// </summary>
        /// <param name="nodeType">Type of the node to create.</param>
        /// <returns>ISyntaxNodeMaker.</returns>
        public static ISynNodeBuilder CreateBuilder(SynNodeType nodeType)
        {
            if (BUILDERS.ContainsKey(nodeType))
                return BUILDERS[nodeType];

            throw new ArgumentOutOfRangeException($"No {typeof(ISynNodeBuilder).FriendlyName()} exists for the node type {nodeType}");
        }
    }
}