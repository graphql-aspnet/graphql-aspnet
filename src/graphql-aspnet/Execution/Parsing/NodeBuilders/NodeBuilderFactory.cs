// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.Parsing.NodeBuilders
{
    using System;
    using System.Collections.Generic;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution.Parsing.NodeBuilders.Inputs;
    using GraphQL.AspNet.Execution.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Interfaces.Execution;

    public class NodeBuilderFactory
    {
        private static readonly Dictionary<SyntaxNodeType, ISyntaxNodeBuilder> BUILDERS;

        /// <summary>
        /// Initializes static members of the <see cref="NodeBuilderFactory"/> class.
        /// </summary>
        static NodeBuilderFactory()
        {
            BUILDERS = new Dictionary<SyntaxNodeType, ISyntaxNodeBuilder>();

            BUILDERS.Add(SyntaxNodeType.Operation, OperationNodeBuilder.Instance);
            BUILDERS.Add(SyntaxNodeType.NamedFragment, NamedFragmentNodeBuilder.Instance);
            BUILDERS.Add(SyntaxNodeType.Variable, VariableNodeBuilder.Instance);
            BUILDERS.Add(SyntaxNodeType.VariableCollection, VariableCollectionNodeBuilder.Instance);
            BUILDERS.Add(SyntaxNodeType.FieldCollection, FieldCollectionNodeBuilder.Instance);
            BUILDERS.Add(SyntaxNodeType.InputValue, InputValueNodeBuilder.Instance);
            BUILDERS.Add(SyntaxNodeType.InputItem, InputItemNodeBuilder.Instance);
            BUILDERS.Add(SyntaxNodeType.InputItemCollection, InputItemCollectionNodeBuilder.Instance);
            BUILDERS.Add(SyntaxNodeType.Directive, DirectiveNodeBuilder.Instance);
        }

        /// <summary>
        /// Creates the maker.
        /// </summary>
        /// <param name="nodeType">Type of the node to create.</param>
        /// <returns>ISyntaxNodeMaker.</returns>
        public static ISyntaxNodeBuilder CreateBuilder(SyntaxNodeType nodeType)
        {
            if (BUILDERS.ContainsKey(nodeType))
                return BUILDERS[nodeType];

            throw new ArgumentOutOfRangeException($"No {typeof(ISyntaxNodeBuilder).FriendlyName()} exists for the node type {nodeType}");
        }
    }
}