// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Engine
{
    using System;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.Parsing;
    using GraphQL.AspNet.Execution.Parsing.Lexing.Source;
    using GraphQL.AspNet.Execution.QueryPlans.Document;
    using GraphQL.AspNet.Execution.RulesEngine;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document;
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// An object capable of creating and validating a query document that can be executed
    /// to generate a graphql result.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this generator is registered for.</typeparam>
    public class DefaultGraphQueryDocumentGenerator<TSchema> : IGraphQueryDocumentGenerator<TSchema>
        where TSchema : class, ISchema
    {
        private readonly TSchema _schema;
        private readonly DocumentConstructionRuleProcessor _nodeProcessor;
        private readonly GraphQLParser _parser;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultGraphQueryDocumentGenerator{TSchema}" /> class.
        /// </summary>
        /// <param name="schema">The schema to use when generating validating a document.</param>
        public DefaultGraphQueryDocumentGenerator(TSchema schema)
        {
            _schema = Validation.ThrowIfNullOrReturn(schema, nameof(schema));
            _nodeProcessor = new DocumentConstructionRuleProcessor();
            _parser = new GraphQLParser();
        }

        /// <inheritdoc />
        public virtual IGraphQueryDocument CreateDocument(ReadOnlySpan<char> queryText)
        {
            var sourceText = new SourceText(queryText);
            var syntaxTree = _parser.CreateSyntaxTree(ref sourceText);

            try
            {
                Validation.ThrowIfNull(syntaxTree.RootNode, nameof(syntaxTree.RootNode));

                var document = this.CreateNewDocumentInstance();
                this.FillDocument(document, syntaxTree, sourceText);
                return document;
            }
            finally
            {
                SyntaxTreeOperations.Release(ref syntaxTree);
            }
        }

        /// <inheritdoc />
        public virtual bool ValidateDocument(IGraphQueryDocument document)
        {
            Validation.ThrowIfNull(document, nameof(document));

            var docProcessor = new DocumentValidationRuleProcessor();
            var context = new DocumentValidationContext(_schema, document);

            return docProcessor.Execute(context);
        }

        /// <summary>
        /// A factory method to generate a new <see cref="IGraphQueryDocument"/>.
        /// This method should just instantiate a document, not perform any work
        /// against it.
        /// </summary>
        /// <returns>IGraphQueryDocument.</returns>
        protected virtual IGraphQueryDocument CreateNewDocumentInstance()
        {
            return new QueryDocument();
        }

        /// <summary>
        /// An internal method for populating a query document with document parts from
        /// a provided syntax tree.
        /// </summary>
        /// <param name="document">The query document to fill.</param>
        /// <param name="syntaxTree">The syntax tree to convert into document parts.</param>
        /// <param name="sourceText">The source text to extract referenced
        /// values from.</param>
        private void FillDocument(IGraphQueryDocument document, SyntaxTree syntaxTree, SourceText sourceText)
        {
            Validation.ThrowIfNull(syntaxTree.RootNode, nameof(syntaxTree.RootNode));
            Validation.ThrowIfNull(document, nameof(document));

            // --------------------------------------------
            // Step 1: Parse the syntax tree
            // --------------------------------------------
            // Walk all nodes of the tree and on a "per node" basis perform actions
            // that are required of that node to create pieces (IDocumentPart) of the
            // document being constructed
            // --------------------------------------------
            var constructionContext = new DocumentConstructionContext(syntaxTree, sourceText, document, _schema);

            var completedAllSteps = _nodeProcessor.Execute(ref constructionContext);
            if (!completedAllSteps)
                return;

            // --------------------------------------------
            // Step 2: Fragment Linking
            // --------------------------------------------
            // Fragment spreads refernece other document parts (named fragments).
            // Ensure those linkages are wired up properly.
            // --------------------------------------------
            foreach (var spread in constructionContext.Spreads)
            {
                if (spread.Fragment != null)
                {
                    spread.Fragment.MarkAsReferenced();
                    continue;
                }

                // its possible that the query text contains multiple named fragments
                // with the same name (the document is not validated yet)
                // the query document will contain these duplicated named fragments
                //
                // mark all fragments with the name in the spread as being referenced
                document.NamedFragments.MarkAsReferenced(spread.FragmentName);

                // assign the officially "chosen" named fragment reference to the spread
                if (document.NamedFragments.TryGetValue(spread.FragmentName, out var foundFragment))
                {
                    spread.AssignNamedFragment(foundFragment);
                }
            }
        }
    }
}