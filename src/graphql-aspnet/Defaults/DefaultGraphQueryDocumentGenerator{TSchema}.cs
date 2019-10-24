// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Defaults
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Fragments;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.ValidationRules;

    /// <summary>
    /// Validates that a lexed syntax tree, intended to execute a query on a target schema,
    /// is valid on that schema such that all field selection sets for all graph types are valid and executable
    /// ultimately generating a document that can be used to create a query plan from the various resolvers
    /// that can complete the query.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this generator is registered for.</typeparam>
    public class DefaultGraphQueryDocumentGenerator<TSchema> : IGraphQueryDocumentGenerator<TSchema>
        where TSchema : class, ISchema
    {
        private readonly TSchema _schema;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultGraphQueryDocumentGenerator{TSchema}" /> class.
        /// </summary>
        /// <param name="schema">The schema to use when generating validating a document.</param>
        public DefaultGraphQueryDocumentGenerator(TSchema schema)
        {
            _schema = Validation.ThrowIfNullOrReturn(schema, nameof(schema));
        }

        /// <summary>
        /// Interpretes the syntax tree and generates a contextual document that can be transformed into
        /// a query plan.
        /// </summary>
        /// <param name="syntaxTree">The syntax tree to create a document for.</param>
        /// <returns>IGraphQueryDocument.</returns>
        public IGraphQueryDocument CreateDocument(ISyntaxTree syntaxTree)
        {
            Validation.ThrowIfNull(syntaxTree, nameof(syntaxTree));

            // --------------------------------------------
            // Step 1: Parse the syntax tree
            // --------------------------------------------
            // Walk all nodes of the tree and on a "per node" basis perform actions
            // that are required of that node be it a specification validation rule or
            // an incremental addition to the document context being built.
            //
            // Note: All packages are rendered and then named fragment nodes are processed first
            //       as they are required by any operations referenced elsewhere in the document
            // --------------------------------------------
            var nodeProcessor = new DocumentConstructionRuleProcessor();
            var docContext = new DocumentContext(_schema);
            var nodeContexts = new List<DocumentConstructionContext>();
            foreach (var node in syntaxTree.Nodes)
            {
                var nodeContext = docContext.ForTopLevelNode(node);
                nodeContexts.Add(nodeContext);
            }

            nodeContexts.Sort(new TopLevelNodeProcessingOrder());
            var completedAllSteps = nodeProcessor.Execute(nodeContexts);

            // --------------------------------------------
            // Step 2: Validate the document parts
            // --------------------------------------------
            // Inspect the document parts that were generated during part one and, as a whole, run additional
            // validation rules and perform final changes before constructing the final document.
            // e.g. ensure all fragments were called, all variables were referenced at least once etc.
            // --------------------------------------------
            if (completedAllSteps)
            {
                var documentProcessor = new DocumentValidationRuleProcessor();
                var validationContexts = new List<DocumentValidationContext>();
                foreach (var part in docContext.Children)
                {
                    var partContext = new DocumentValidationContext(docContext, part);
                    validationContexts.Add(partContext);
                }

                documentProcessor.Execute(validationContexts);
            }

            // --------------------------------------------
            // Step 3: Build out the final document
            // --------------------------------------------
            return docContext.ConstructDocument();
        }

        /// <summary>
        /// Sorts a collection of construction packages such that those referencing a <see cref="NamedFragmentNode"/>
        /// are at the top of the list.
        /// </summary>
        private class TopLevelNodeProcessingOrder : IComparer<DocumentConstructionContext>
        {
            /// <summary>
            /// Compares the two packages for sortability.
            /// </summary>
            /// <param name="x">The first package to compare.</param>
            /// <param name="y">The second package to compare.</param>
            /// <returns>System.Int32.</returns>
            public int Compare(DocumentConstructionContext x, DocumentConstructionContext y)
            {
                if (x?.ActiveNode == null)
                {
                    return y?.ActiveNode is NamedFragmentNode ? 1 : 0;
                }

                if (y?.ActiveNode == null)
                {
                    return x?.ActiveNode is NamedFragmentNode ? -1 : 0;
                }

                if (x.ActiveNode is NamedFragmentNode && y.ActiveNode is NamedFragmentNode)
                    return 0;

                if (x.ActiveNode is NamedFragmentNode)
                    return -1;

                if (y.ActiveNode is NamedFragmentNode)
                    return 1;

                return 0;
            }
        }
    }
}